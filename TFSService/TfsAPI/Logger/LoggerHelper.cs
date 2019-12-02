using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TfsAPI.Logger
{
    public class LoggerHelper
    {
        public static void WriteLine(object message,
            [CallerMemberName] string caller = null,
            [CallerFilePath] string file = null,
            [CallerLineNumber] int line = -1)
        {
            var now = DateTime.Now;

            var lineNumber = line < 0
                ? string.Empty
                : $" ({line})";

            if (!string.IsNullOrEmpty(file) && file.Contains("\\"))
            {
                var index = file.LastIndexOf("\\", StringComparison.Ordinal) + 1;
                file = file.Substring(index, file.Length - index);
            }

            Trace.WriteLine($"{now.ToShortDateString()} {now:hh.mm.ss.fff} [{file}::{caller}{lineNumber}] {message}");
        }

        public static void WriteLineIf(bool condition, object message,
            [CallerMemberName] string caller = null,
            [CallerFilePath] string file = null,
            [CallerLineNumber] int line = -1)
        {
            if (condition)
            {
                WriteLine(message, caller, file, line);
            }
        }
    }
}