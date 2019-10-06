using Discord;
using Silicon.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Silicon.Helpers
{
    static class LoggingHelper
    {
        private static readonly TextWriter writer = new StreamWriter(File.Open("./log.txt", FileMode.Create));

        public static Task Log(LogSeverity severity, LogSource source, string msg, Exception e = null)
        {
            LogInternal(severity, source, msg, e);
            return Task.CompletedTask;
        }

        public static Task Log(LogMessage m)
        {
            LogInternal(m.Severity, m.GetLogSrc(), m.Message, m.Exception);
            return Task.CompletedTask;
        }

        private static void LogInternal(LogSeverity severity, LogSource source, string msg, Exception e)
        {
            var color = VerifySeverity(severity);
            AppendText(severity.ToString(), color);
            AppendText("->", ConsoleColor.White);

            color = VerifySource(source);
            AppendText(source.ToString(), color);
            AppendText(":", ConsoleColor.White);

            writer.Write($"{severity} -> {source} : ");

            if (!msg.IsNullOrWhitespace())
            {
                AppendText(msg, ConsoleColor.White);
                writer.Write(msg);
            }

            if (e != null)
            {
                AppendText($"{e.Message}\n{e.StackTrace}", VerifySeverity(severity));
                writer.Write($"{e.Message}\n{e.StackTrace}");
            }

            Console.WriteLine();
            Console.ResetColor();
            writer.WriteLine();

            static void AppendText(string text, ConsoleColor color)
            {
                Console.ForegroundColor = color;
                Console.Write(text + " ");
            }
        }

        private static ConsoleColor VerifySeverity(LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    return ConsoleColor.DarkRed;

                case LogSeverity.Warning:
                    return ConsoleColor.Yellow;

                case LogSeverity.Info:
                    return ConsoleColor.White;

                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                default:
                    return ConsoleColor.Gray;
            }
        }

        private static ConsoleColor VerifySource(LogSource source)
        {
            switch (source)
            {
                case LogSource.Module:
                case LogSource.Service:
                    return ConsoleColor.Yellow;

                case LogSource.Discord:
                case LogSource.Rest:
                case LogSource.Gateway:
                    return ConsoleColor.Blue;

                case LogSource.Silicon:
                    return ConsoleColor.Green;

                case LogSource.Unknown:
                default:
                    return ConsoleColor.Gray;
            }
        }
    }
}
