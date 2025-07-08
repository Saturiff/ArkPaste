namespace ArkScriptEditor.Classes
{
    internal class Script
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public ScriptState State { get; set; }

        public string ToString()
        {
            return string.Format("名稱：{0}\n描述：{1}", Name, Desc);
        }
    }

    enum ScriptState
    {
        Idle,
        Running,
    }
}
