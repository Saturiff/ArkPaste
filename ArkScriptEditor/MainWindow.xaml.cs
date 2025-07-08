using ArkScriptEditor.Classes;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace ArkScriptEditor
{
    public partial class MainWindow : Window
    {
        private static readonly LuaScriptReader scriptReader = new LuaScriptReader();

        private static readonly List<Script> scripts = [];

        private Script currentScript;

        public MainWindow()
        {
            InitializeComponent();
            
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

        private void Check_StartScript_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            bool running = checkBox.IsChecked ?? false;
            currentScript.State = running ? ScriptState.Running : ScriptState.Idle;
        }

        private void List_Script_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            currentScript = (Script)listView.SelectedItem;
            Text_ScriptInfo.Text = currentScript.Name;

            if (!Check_StartScript.IsEnabled)
            {
                Check_StartScript.IsEnabled = true;
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
            string[] filePaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.lua");
            foreach (string path in filePaths)
            {
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
        }
    }
}
