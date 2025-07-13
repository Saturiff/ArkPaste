using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace ArkScriptEditor.Classes
{
    internal class HotKeySetting
    {
        public required int ID { get; set; }
        public required Key DefaultKey { get; set; }
        public required Action<Key> HotKeyBindedCallback { get; set; }
        public required Action HotKeyPressedCallback { get; set; }
        public required Func<Key> SettingGet { get; set; }
    }

    internal static class HotKey
    {
        // Global hot key hook in Windows
        // https://stackoverflow.com/a/76865142
        // https://blog.magnusmontin.net/2015/03/31/implementing-global-hot-keys-in-wpf/

        private static DateTime hotKeyLastTriggerTime = DateTime.MinValue;
        private static readonly Dictionary<int, HotKeySetting> hotKeyMap = [];

        // Modifiers:
        private const uint MOD_NONE = 0x0000;

        private static IntPtr _windowHandle;
        private static HwndSource _source;

        public static void Initialize(Window w, HotKeySetting[] hotkeyData)
        {
            _windowHandle = new WindowInteropHelper(w).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            hotKeyMap.Clear();
            foreach (var hkSetting in hotkeyData)
            {
                hotKeyMap.Add(hkSetting.ID, hkSetting);

                Key savedKey = hkSetting.SettingGet.Invoke();
                if (savedKey == Key.None)
                {
                    savedKey = hkSetting.DefaultKey;
                }

                SetHotKey(hkSetting.ID, savedKey);
            }
        }

        public static void Dispose()
        {
            _source.RemoveHook(HwndHook);
            foreach (var hotKeyID in hotKeyMap.Keys)
            {
                ScriptLibrary.UnregisterHotKey(_windowHandle, (int)hotKeyID);
            }
            hotKeyMap.Clear();
        }

        public static void SetHotKey(int hotKeyID, Key newKey)
        {
            // Is re-initialize check

            if (_source != null)
            {
                ScriptLibrary.UnregisterHotKey(_windowHandle, hotKeyID);
            }

            // Register key

            uint vk = (uint)KeyInterop.VirtualKeyFromKey(newKey);
            bool success = ScriptLibrary.RegisterHotKey(_windowHandle, hotKeyID, MOD_NONE, vk);
            if (success)
            {
                Logger.Info("HotKey", string.Format("全局熱鍵掛勾成功: {0}", hotKeyID));
                hotKeyMap[hotKeyID].HotKeyBindedCallback(newKey);
            }
            else
            {
                Logger.Error("HotKey", string.Format("全局熱鍵掛勾失敗，目標快捷鍵可能已經被其他軟體註冊 (last_err={0})", Marshal.GetLastWin32Error()));
            }
        }

        private static IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
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
                    Key key = KeyInterop.KeyFromVirtualKey(vkey);

                    int hotkeyID = wParam.ToInt32();
                    var setting = hotKeyMap[hotkeyID];
                    var savedKey = setting.SettingGet.Invoke();

                    if (key == savedKey)
                    {
                        hotKeyMap[hotkeyID].HotKeyPressedCallback.Invoke();
                        handled = true;
                    }

                    break;
            }
            return IntPtr.Zero;
        }
    }
}
