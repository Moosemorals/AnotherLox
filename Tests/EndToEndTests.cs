using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Lib;

using Xunit;

namespace Tests {
    public class EndToEndTests {
        [Theory]
        [MemberData(nameof(LanguageTestCases))]
        public void LanguageTests(string input, object? expected) {

            IErrorReporter _log = new BlackholeLogger();

            Scanner scanner = new Scanner(_log, input);
            IList<Token> tokens = scanner.ScanTokens();

            Parser parser = new Parser(_log, tokens);
            Expr? expr = parser.Parse();

            Assert.NotNull(expr);

            if (expr == null) {
                return;
            }
            LambdaExpression? lambda = new Interpreter(_log).Interpret(expr);
            Assert.NotNull(lambda);
            if (lambda == null) {
                return;
            }
            object? actual = lambda.Compile().DynamicInvoke();
            Assert.Equal(expected, actual);
        }


        public static IEnumerable<object[]> LanguageTestCases() {

            yield return new object[] { "nil", null! };
            yield return new object[] { "true", true };
            yield return new object[] { "false",false };
            yield return new object[] { "\"2\"", "2" };
            yield return new object[] { "4 + 4", 8d };
            yield return new object[] { "6 / 2", 3d };
            yield return new object[] { "7 / 2", 3.5d };
            yield return new object[] { "7 > 2", true };
            yield return new object[] { "7 < 2", false };
            yield return new object[] { "7 >= 2", true };
            yield return new object[] { "7 <= 2", false };
            yield return new object[] { "(6 + 6) / (5 - 2)", 4d };
            yield return new object[] { "-7", -7d };
            yield return new object[] { "5 + -7", -2d };
            yield return new object[] { "!true", false };
            yield return new object[] { "!false", true };
            yield return new object[] { "\"a\" == \"a\"", true };
            yield return new object[] { "\"a\" != \"b\"", true };
            yield return new object[] { "!(\"a\" != \"b\")", false };
            yield return new object[] { "4 == 5", false };
            yield return new object[] { "5 == 5", true };
            yield return new object[] { "5 == true", false };
            yield return new object[] { "\"a\" + \"a\"", "aa" };
        }
    }

    public class BlackholeLogger : IErrorReporter {
        public bool HadError { get; private set; }

        public void Error(string format, params object[] args) {
            // ignore;
        }

        public void Info(string format, params object[] args) {
            // ignore
        }

        public void Reset() => HadError = false;
        public void Warning(string format, params object[] args) {
            // ignore
        }
    }
}
