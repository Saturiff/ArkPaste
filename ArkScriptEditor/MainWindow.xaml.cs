using ArkScriptEditor.Classes;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ArkScriptEditor
{
    public partial class MainWindow : Window
    {
        private static readonly LuaScriptReader scriptReader = new LuaScriptReader();

        private static readonly List<Script> scripts = [];

        private static readonly Dictionary<string, ScriptRunner> scriptRunners = [];

        public MainWindow()
        {
            InitializeComponent();

            B_AddScript.Click += B_AddScript_Click;
            B_RefreshScript.Click += B_RefreshScript_Click;

            // lua script info group box

            Text_ScriptInfo.Text = "";
            List_Script.SelectionChanged += List_Script_OnSelectionChanged;
            Check_StartScript.Checked += Check_StartScript_StateUpdated;
            Check_StartScript.Unchecked += Check_StartScript_StateUpdated;
            Check_StartScript.IsEnabled = false;

            TB_ActionViewer.Text = "選擇一個腳本";

            // log textbox

            TB_Log.Clear();
            Logger.Log += Logger_OnLog;
            Logger.Info(this, "Logger ready");

            // initialize script tabs

            ScanScriptFiles();
        }

        private void B_AddScript_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void B_RefreshScript_Click(object sender, RoutedEventArgs e)
        {
            Logger.Info(this, "準備重新讀取\n");
            ScanScriptFiles();
        }

        private void Check_StartScript_StateUpdated(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;

            Script? script = GetCurrentScript();
            if (script == null)
            {
                checkBox.IsChecked = false;
                return;
            }

            // TODO: Change tab text color

            ScriptRunner? runner = GetCurrentScriptRunner();
            if (runner == null)
            {
                checkBox.IsChecked = false;
                return;
            }

            if (checkBox.IsChecked ?? false)
            {
                if (!runner.Start())
                {
                    checkBox.IsChecked = false;
                }
            }
            else
            {
                runner.Stop();
            }

            Text_ScriptInfo.Text = script.ToString();
        }

        private void List_Script_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Script? script = GetCurrentScript();
            if (script != null)
            {
                Text_ScriptInfo.Text = script.ToString();
                TB_ActionViewer.Text = script.ActionDump;
                Check_StartScript.IsEnabled = true;
                Check_StartScript.IsChecked = script.State == ScriptState.Running;
            }
            else
            {
                Text_ScriptInfo.Text = "";
                TB_ActionViewer.Text = "選擇一個腳本";
                Check_StartScript.IsEnabled = false;
                Check_StartScript.IsChecked = false;
            }
        }

        private void Logger_OnLog(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                string senderStr = "Unknow";
                if (sender != null)
                {
                    string[] senderPath = sender.ToString().Split('.');
                    senderStr = senderPath[^1];
                }

                TB_Log.Text += string.Format("[{0}] {1}\n", senderStr, ((LoggerEventArgs)e).ToString());
                TB_Log.ScrollToEnd();
            });
        }

        private void ScanScriptFiles()
        {
            if (scripts.Count > 0)
            {
                Logger.Info(this, "Rescaning...");
                scriptReader.Clear();

                var runners = scriptRunners.Values;
                foreach (var runner in runners)
                {
                    runner?.Dispose();
                }

                scripts.Clear();
            }

            string scriptDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Script");
            string[] filePaths = Directory.GetFiles(scriptDir, "*.lua");
            foreach (string path in filePaths)
            {
                if (scriptReader.LoadScriptHide(path))
                {
                    Logger.Info(this, string.Format("略過了一個隱藏的腳本"));
                    continue;
                }

                string name = Path.GetFileNameWithoutExtension(path);
                string desc = scriptReader.LoadScriptDescription(path);
                if (desc == "")
                {
                    desc = name;
                }
                var actions = scriptReader.LoadScriptActions(path, out string actionDump);

                Script script = new Script
                {
                    Path = path,
                    Name = name,
                    Desc = desc,
                    State = ScriptState.Idle,
                    Actions = actions,
                    ActionDump = actionDump,
                    ActionInterval = scriptReader.LoadScriptGlobalDelay(path),
                };
                scripts.Add(script);
                scriptRunners[script.Name] = new ScriptRunner(script);

                Logger.Info(this, string.Format("找到了腳本: {0} [{1}]", desc, name));
            }
            List_Script.ItemsSource = scripts;
        }

        private Script? GetCurrentScript()
        {
            Script? script = (Script)List_Script.SelectedItem;
            if (script == null)
            {
                Logger.Warn(this, string.Format("GetCurrentScript when script is null. Selected: {0}", List_Script.SelectedItem));
            }
            return script;
        }

        private ScriptRunner? GetCurrentScriptRunner()
        {
            var script = GetCurrentScript();
            if (script == null)
            {
                Logger.Warn(this, string.Format("在沒有腳本時試圖取得Runner"));
                return null;
            }

            if (scriptRunners.TryGetValue(script.Name, out ScriptRunner? runner) && runner != null)
            {
                return runner;
            }
            else
            {
                Logger.Error(this, string.Format("腳本 {0} 的 runner 不存在", script.Name));
                runner = new ScriptRunner(script);
                scriptRunners[script.Name] = runner;
                return runner;
            }
        }
    }
}
