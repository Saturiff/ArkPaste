using System.ComponentModel;

namespace ArkScriptEditor.Classes
{
    internal class Script : INotifyPropertyChanged
    {
        public required string Path { get; set; }
        public required string Name { get; set; }
        public required string Desc { get; set; }
        private ScriptState state;
        public ScriptState State {
            get => state;
            set
            {
                state = value;
                OnPropertyChanged(nameof(State));
            }
        }
        public required List<IScriptAction> Actions { get; set; }
        public required string ActionDump { get; set; }
        public int ActionInterval { get; set; } = 16;


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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
