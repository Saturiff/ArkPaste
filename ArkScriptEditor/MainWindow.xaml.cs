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

        public MainWindow()
        {
            InitializeComponent();

            B_AddScript.Click += B_AddScript_Click;
            B_RefreshScript.Click += B_RefreshScript_Click;

            // lua script info group box

            Text_ScriptInfo.Text = "";
            List_Script.SelectionChanged += List_Script_OnSelectionChanged;
            Check_StartScript.Checked += Check_StartScript_Checked;
            Check_StartScript.IsEnabled = false;

            // log textbox

            TB_Log.Clear();
            Logger.Log += Logger_OnLog;
            Logger.Info(this, "Logger ready");

            // initialize script tabs

            ScanScriptFiles();

            // TODO: select first script as default if have

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

        private void Check_StartScript_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            bool running = checkBox.IsChecked ?? false;
            Script? script = GetCurrentScript();
            if (script != null)
            {
                script.State = running ? ScriptState.Running : ScriptState.Idle;
            }

            // TODO: Change tab text color

            // TODO: Run/Stop runner script

        }

        private void List_Script_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Script? script = GetCurrentScript();
            if (script != null)
            {
                Text_ScriptInfo.Text = script.ToString();

                
                List<IScriptAction> actions = scriptReader.LoadScriptActions(script.Path);
                // TODO: Init runner with actions
            }
            else
            {
                Text_ScriptInfo.Text = "";
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

                // TODO: Stop/Clear all running script

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

                Script script = new Script
                {
                    Path = path,
                    Name = name,
                    Desc = desc,
                    State = ScriptState.Idle,
                };
                scripts.Add(script);

                Logger.Info(this, string.Format("找到了腳本: {0} [{1}]", desc, name));
            }
            List_Script.ItemsSource = scripts;

            if (scripts.Count > 0)
            {
                List_Script.SelectedItem = scripts[0];
                Check_StartScript.IsEnabled = true;
            }
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
    }
}
