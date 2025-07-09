namespace ArkScriptEditor.Classes
{
    internal class Script
    {
        public required string Path { get; set; }
        public required string Name { get; set; }
        public required string Desc { get; set; }
        public ScriptState State { get; set; }
        public required List<IScriptAction> Actions { get; set; }
        public required string ActionDump { get; set; }
        public int ActionInterval { get; set; } = 50;

        public override string ToString()
        {
            return string.Format("名稱：{0}\n描述：{1}\n狀態：{2}\n每條指令執行間隔：{3}", Name, Desc, State, ActionInterval);
        }

    }

    enum ScriptState
    {
        Idle,
        Running,
    }
}
