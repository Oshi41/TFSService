using System;
using System.Diagnostics;
using System.IO;
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

            if (!string.IsNullOrEmpty(file))
            {
                file = Path.GetFileNameWithoutExtension(file);
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