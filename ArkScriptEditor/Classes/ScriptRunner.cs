using Timer = System.Windows.Forms.Timer;

namespace ArkScriptEditor.Classes
{
    class ScriptRunner(Script script)
    {
        public long DeltaMillisecond { get; private set; } = 0;
        public nint HWnd { get; private set; } = 0;

        private Timer? timer;
        private int currentActionIndex;

        public bool Start()
        {
            if (script == null)
            {
                Logger.Error(this, "腳本執行失敗，ScriptRunner沒有被正確初始化");
                return false;
            }

            if (script.Actions == null)
            {
                Logger.Error(this, "腳本未被正確初始化");
                return false;
            }

            if (script.Actions.Count == 0)
            {
                Logger.Warn(this, "這是一個空腳本，因此不會執行");
                return false;
            }

            HWnd = ScriptLibrary.FindWindow(null, "ARK: Survival Evolved");
            if (HWnd == 0)
            {
                Logger.Error(this, "腳本執行失敗，無法找到遊戲視窗 (ARK: Survival Evolved)");
                return false;
            }

            if (timer == null)
            {
                timer = new Timer();
                timer.Tick += OnTimerTick;
                DeltaMillisecond = 0;
                currentActionIndex = 0;
            }

            timer.Interval = script.ActionInterval;
            timer.Start();
            script.State = ScriptState.Running;

            return true;
        }

        public void Stop()
        {
            timer?.Stop();
            script.State = ScriptState.Idle;
        }

        public void Dispose()
        {
            Stop();
            timer?.Dispose();
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            if (timer == null)
            {
                Stop();
                Logger.Error(this, "stop script: timer invalid");
                return;
            }
            DeltaMillisecond += timer.Interval;

            var actions = script.Actions ?? [];
            if (currentActionIndex >= actions.Count)
            {
                currentActionIndex = 0;
            }

            if (actions[currentActionIndex].Execute(this))
            {
                currentActionIndex++;
            }

            return;
        }
    }
}
