using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;

using Lib;

namespace App {
    class Program {

        private readonly static IErrorReporter _log = new ConsoleErrorReporter();

        private static void RunFile(string path) {
            Run(File.ReadAllText(path, Encoding.UTF8));
            if (_log.HadError) {
                return;
            }
        }

        private static void RunPrompt() {
            while (true) {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line)) {
                    Run(line);
                }
                _log.Reset();
            }
        }

        private static void Run(string source) {
            Scanner scanner = new Scanner(_log, source);
            IList<Token> tokens = scanner.ScanTokens();

            Parser parser = new Parser(_log, tokens);
            Expr? expr = parser.Parse();

            if (expr == null) {
                return;
            }

            _log.Info("Parse result: {0}", new PrettyPrinter().Print(expr));
            LambdaExpression? result = new Interpreter(_log).Interpret(expr);
            if (result == null) {
                return;
            }

            _log.Info("Interpret result: {0}", result.ToString());
            _log.Info("Run result: {0}", result.Compile().DynamicInvoke() ?? "nil");
        }

        public static void Main(string[] args) {
            if (args.Length > 1) {
                _log.Error("Usage: jlox [script]");
                return;
            } else if (args.Length == 1) {
                RunFile(args[0]);
            } else {
                RunPrompt();
            }
        }

    }
}
