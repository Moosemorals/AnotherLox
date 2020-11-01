using System;
using System.Collections.Generic;
using System.Text;

namespace Lib {
    public interface IErrorReporter {
        public void Error(string format, params object[] args);
        public void Warning(string format, params object[] args);
        public void Info(string format, params object[] args);

        public bool HadError {get; }

        public void Reset();
    }
}
