namespace ArkScriptEditor.Classes
{
    internal static class Logger
    {
        public static event EventHandler? Log;

        public static void Info(object ctx, string logStr)
        {
            LogImpl(LogLevel.Info, ctx, logStr);
        }

        public static void Warn(object ctx, string logStr)
        {
            LogImpl(LogLevel.Warn, ctx, logStr);
        }

        public static void Error(object ctx, string logStr)
        {
            LogImpl(LogLevel.Error, ctx, logStr);
        }

        private static void LogImpl(LogLevel level, object ctx, string logStr)
        {
            LogItem logItem = new LogItem()
            {
                Level = level,
                Ctx = ctx,
                Message = logStr,
            };

            Console.WriteLine(logItem.ToString());

            LoggerEventArgs args = new LoggerEventArgs(logItem);
            OnLog(ctx, args);
        }

        private static void OnLog(object ctx, LoggerEventArgs args)
        {
            Log?.Invoke(ctx, args);
        }
    }

    class LoggerEventArgs(LogItem logItem) : EventArgs
    {
        private readonly LogItem logItem = logItem;

        public LogItem GetLogItem()
        {
            return logItem;
        }
    }
}
