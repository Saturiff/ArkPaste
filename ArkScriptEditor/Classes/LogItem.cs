namespace ArkScriptEditor.Classes
{
    internal class LogItem
    {
        //object ctx, string logStr
        public required LogLevel Level { get; set; }
        public required object? Ctx { get; set; }
        public required string Message { get; set; }

        public override string ToString()
        {
            string ctxStr = "Unknow";
            if (Ctx != null)
            {
                string[] senderPath = Ctx.ToString().Split('.');
                ctxStr = senderPath[^1];
            }

            string logStr = Level switch
            {
                LogLevel.Info => "資訊",
                LogLevel.Warn => "警告",
                LogLevel.Error => "錯誤",
                _ => Level.ToString(),
            };
            return string.Format("[{0}] {1}: {2}", ctxStr, logStr, Message);
        }
    }

    enum LogLevel
    {
        Info,
        Warn,
        Error,
    }
}
