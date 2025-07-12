using ArkScriptEditor.Classes;
using ArkScriptEditor.Properties;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace ArkScriptEditor
{
    public partial class MainWindow : Window
    {
        private static readonly LuaScriptReader scriptReader = new LuaScriptReader();

        private static readonly ObservableCollection<Script> scripts = [];

        private static readonly ObservableCollection<LogItem> logItems = [];

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

            B_EditKey_ToggleCurrent.Click += B_EditKey_Click;
            B_EditKey_StopAll.Click += B_EditKey_Click;

            TB_ActionViewer.Text = "選擇一個腳本";

            // log textbox

            List_Log.ItemsSource = logItems;
            logItems.CollectionChanged += LogItems_CollectionChanged;
            Logger.Log += Logger_OnLog;
            Logger.Info(this, "Logger ready");

            // initialize script tabs

            List_Script.ItemsSource = scripts;
            ScanScriptFiles();
        }

        private void LogItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (VisualTreeHelper.GetChildrenCount(List_Log) > 0)
            {
                Border border = (Border)VisualTreeHelper.GetChild(List_Log, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }

        // Global hot key hook in Windows
        // https://stackoverflow.com/a/76865142
        // https://blog.magnusmontin.net/2015/03/31/implementing-global-hot-keys-in-wpf/

        enum HotKeyID
        {
            ToggleCurrent = 1000,
            StopAll = 1001,
        }

        private DateTime hotKeyLastTriggerTime = DateTime.MinValue;

        // Modifiers:
        private const uint MOD_NONE = 0x0000;

        private IntPtr _windowHandle;
        private HwndSource _source;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            SetHotKey(HotKeyID.ToggleCurrent);
            SetHotKey(HotKeyID.StopAll);
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            ScriptLibrary.UnregisterHotKey(_windowHandle, (int)HotKeyID.ToggleCurrent);
            ScriptLibrary.UnregisterHotKey(_windowHandle, (int)HotKeyID.StopAll);
            base.OnClosed(e);
        }

        private void SetHotKey(HotKeyID hotKeyID, Key? newKey = null)
        {
            int iHotKeyID = (int)hotKeyID;

            // Check default

            switch (hotKeyID)
            {
                case HotKeyID.ToggleCurrent:
                    newKey ??= Key.F7;
                    break;
                case HotKeyID.StopAll:
                    newKey ??= Key.F8;
                    break;
                default:
                    Logger.Info("HotKey", string.Format("InitializeHotKey: 未實現的 HotKeyID ({0})", hotKeyID));
                    return;
            }

            // Check same

            int iNewKey = (int)newKey;

            int[] bindedKeys = [
                Settings.Default.HotKey_ToggleCurrent,
                Settings.Default.HotKey_StopAll,
            ];
            if (bindedKeys.Contains(iNewKey))
            {
                Logger.Warn("HotKey", "綁定失敗，熱鍵已被綁定在其他地方");
                return;
            }

            // Is re-initialize check

            if (_source != null)
            {
                ScriptLibrary.UnregisterHotKey(_windowHandle, iHotKeyID);
            }
            else
            {
                _windowHandle = new WindowInteropHelper(this).Handle;
                _source = HwndSource.FromHwnd(_windowHandle);
                _source.AddHook(HwndHook);
            }

            // Register key

            uint vk = (uint)KeyInterop.VirtualKeyFromKey(newKey.Value);
            bool success = ScriptLibrary.RegisterHotKey(_windowHandle, iHotKeyID, MOD_NONE, vk);
            if (success)
            {
                Logger.Info("HotKey", string.Format("全局熱鍵掛勾成功: {0} (id={1})", hotKeyID.ToString(), iHotKeyID));
                switch (hotKeyID)
                {
                    case HotKeyID.ToggleCurrent:
                        Settings.Default.HotKey_ToggleCurrent = iNewKey;
                        Text_ToggleCurrentKeyHint.Text = newKey.ToString();
                        break;
                    case HotKeyID.StopAll:
                        Settings.Default.HotKey_StopAll = iNewKey;
                        Text_StopAllKeyHint.Text = newKey.ToString();
                        break;
                    default:
                        Logger.Error("HotKey", string.Format("InitializeHotKey: 未實現的 HotKeyID ({0})", hotKeyID));
                        return;
                }
            }
            else
            {
                Logger.Error("HotKey", string.Format("全局熱鍵掛勾失敗，目標快捷鍵可能已經被其他軟體註冊 (last_err={0})", Marshal.GetLastWin32Error()));
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    var now = DateTime.Now;
                    // 0.3 Cooldown
                    if ((now - hotKeyLastTriggerTime).TotalSeconds < 0.3)
                    {
                        break;
                    }
                    hotKeyLastTriggerTime = now;

                    int vkey = ((int)lParam >> 16) & 0xFFFF;
                    int iKey = (int)KeyInterop.KeyFromVirtualKey(vkey);
                    switch (wParam.ToInt32())
                    {
                        case (int)HotKeyID.ToggleCurrent:
                            if (iKey == Settings.Default.HotKey_ToggleCurrent)
                            {
                                if (EditingHotKey != null)
                                {
                                    Logger.Warn("HotKey", "綁定失敗，熱鍵已被綁定在其他地方");
                                    break;
                                }

                                Logger.Info(this, "切換當前腳本的狀態");
                                if (List_Script.SelectedItem != null)
                                {
                                    Check_StartScript.IsChecked = !Check_StartScript.IsChecked;
                                }
                            }
                            handled = true;
                            break;
                        case (int)HotKeyID.StopAll:
                            if (iKey == Settings.Default.HotKey_StopAll)
                            {
                                if (EditingHotKey != null)
                                {
                                    Logger.Warn("HotKey", "綁定失敗，熱鍵已被綁定在其他地方");
                                    break;
                                }

                                Logger.Info(this, "停下全部腳本");
                                var runners = scriptRunners.Values;
                                foreach (var runner in runners)
                                {
                                    runner?.Stop();
                                }
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void B_AddScript_Click(object sender, RoutedEventArgs e)
        {
            Logger.Info(this, "複製本地的 Script\\template.lua 文件用來製作自己的腳本，若已經遺失請至下方連結索取");
            Logger.Info(this, "線上腳本範本連結:");
            Logger.Info(this, "    https://github.com/Saturiff/ArkPaste/blob/master/ArkScriptEditor/Script/template.lua");
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
                LoggerEventArgs args = ((LoggerEventArgs)e);
                var item = args.GetLogItem();
                logItems.Add(item);
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
        }

        private Script? GetCurrentScript()
        {
            if (List_Script.SelectedItem == null)
            {
                return null;
            }
            return (Script)List_Script.SelectedItem;
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

        // 當按鍵已經存在時 -> end
        private Button? EditingHotKey = null;

        private void B_EditKey_Click(object sender, RoutedEventArgs e)
        {
            EndKeyBinding();

            Logger.Info(this, string.Format("進入按鍵綁定模式，按下 {0} 停止", Key.Escape));
            EditingHotKey = (Button)sender;
            EditingHotKey.Focus();
            EditingHotKey.PreviewKeyDown += B_EditKey_PreviewKeyDown;
            EditingHotKey.LostFocus += B_EditKey_LostFocus;
        }

        private void B_EditKey_LostFocus(object sender, RoutedEventArgs e)
        {
            EndKeyBinding();
        }

        private void B_EditKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                EndKeyBinding();
                return;
            }

            if (sender == B_EditKey_ToggleCurrent)
            {
                SetHotKey(HotKeyID.ToggleCurrent, e.Key);
            }
            else if (sender == B_EditKey_StopAll)
            {
                SetHotKey(HotKeyID.StopAll, e.Key);
            }
            else
            {
                Logger.Error(this, string.Format("非預期的發布者: {0}", sender.ToString()));
            }

            EndKeyBinding();
        }

        void EndKeyBinding()
        {
            if (EditingHotKey != null)
            {
                EditingHotKey.PreviewKeyDown -= B_EditKey_PreviewKeyDown;
                EditingHotKey.LostFocus -= B_EditKey_LostFocus;
                EditingHotKey = null;
            }
            Grid_Main.Focus();
        }
    }
}
