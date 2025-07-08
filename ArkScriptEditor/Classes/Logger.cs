namespace ArkScriptEditor.Classes
{
    internal static class Logger
    {
        public static event EventHandler Log;

        public static void Info(object ctx, string logStr)
        {
            LogImpl(ctx, string.Format("資訊: {0}", logStr));
        }

        public static void Warn(object ctx, string logStr)
        {
            LogImpl(ctx, string.Format("警告: {0}", logStr));
        }

        public static void Error(object ctx, string logStr)
        {
            LogImpl(ctx, string.Format("錯誤: {0}", logStr));
        }

        private static void LogImpl(object ctx, string content)
        {
            Console.WriteLine(content);

            LoggerEventArgs args = new LoggerEventArgs(content);
            OnLog(ctx, args);
        }

        private static void OnLog(object ctx, LoggerEventArgs args)
        {
            Log?.Invoke(ctx, args);
        }
    }

    class LoggerEventArgs : EventArgs
    {
        private string content;
        
        public LoggerEventArgs(string content)
        {
            this.content = content;
        }

        public string ToString()
        {
            return content;
        }
    }
}
