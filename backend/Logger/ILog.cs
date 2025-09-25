using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLogger {
    public interface ILog {
        public static abstract void Init();

        public static abstract void Fatal(string text, Exception? exception = null);

        public static abstract void Error(string text, Exception? exception = null);

        public static abstract void Warning(string text, Exception? exception = null);
        
        public static abstract void Information(string text);

        public static abstract void Debug(string text);
    }
}
