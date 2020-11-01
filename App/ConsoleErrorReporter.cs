using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

using Lib;

namespace App {
    internal class ConsoleErrorReporter : IErrorReporter {
        
        public bool HadError { get; private set; }
        
        public void Error(string format, params object[] args)  {
            Console.WriteLine("Error: " + format, args);
            HadError = true;
        }
        public void Warning(string format, params object[] args) => Console.WriteLine("Warning: " + format, args);
        public void Info(string format, params object[] args) => Console.WriteLine("Info: " + format, args);

        public void Reset() => HadError = false;
    }
}
