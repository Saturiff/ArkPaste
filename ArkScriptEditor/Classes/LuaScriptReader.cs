using NLua;
using System.IO;
using System.Text;

namespace ArkScriptEditor.Classes
{
    internal class LuaScriptReader
    {
        public LuaScriptReader() { }
        ~LuaScriptReader()
        {
            Clear();
        }

        private readonly Dictionary<string, Lua> openedLua = [];

        public void Clear()
        {
            foreach (string scriptPath in openedLua.Keys)
            {
                Close(scriptPath);
            }
        }

        public string LoadScriptDescription(string scriptPath)
        {
            Lua? lua = LoadLua(scriptPath);
            if (lua == null)
            {
                Logger.Error(this, "無法讀取描述字串 (腳本異常)");
                return "";
            }

            string desc = (string)lua["Description"];
            if (desc == null)
            {
                Logger.Error(this, "無法讀取描述字串: 請在 Lua script 中加入 Description\n範例:\nDescription = \"將此替換成描述文字\"");
                return "";
            }

            return desc;
        }

        public int LoadScriptGlobalDelay(string scriptPath)
        {
            Lua? lua = LoadLua(scriptPath);
            if (lua == null)
            {
                Logger.Error(this, "無法讀取全局延遲 (腳本異常)");
                return 20;
            }

            int value = (int)lua["GlobalDelay"];
            return Math.Clamp(value, 20, value);
        }

        public bool LoadScriptHide(string scriptPath)
        {
            Lua? lua = LoadLua(scriptPath);
            if (lua == null)
            {
                Logger.Error(this, "無法讀取隱藏設定 (腳本異常)");
                return false;
            }

            bool value = (bool)lua["Hide"];
            return value;
        }

        public void LoadScriptRun(string scriptPath)
        {
            Lua? lua = LoadLua(scriptPath);
            if (lua == null)
            {
                Logger.Error(this, "無法讀取 Run 方法 (腳本異常)");
                return;
            }

            LuaFunction runFunc = (LuaFunction)lua["Run"];
            if (runFunc == null)
            {
                Logger.Error(this, "無法讀取 Run 方法 (請檢查方法是否存在與合法)");
                return;
            }

            object[] runData = runFunc.Call();

            ResolveActionTable(runData[0]);

            return;
        }

        private void ResolveActionTable(object tableObject, int depth = 0)
        {
            string indent = new string(' ', depth * 4);

            LuaTable actions = (LuaTable)tableObject;
            if (actions.Values.Count == 0)
            {
                Logger.Info(this, string.Format("{0}這是一個空的動作清單", indent));
                return;
            }

            foreach (LuaTable action in actions.Values)
            {
                string type = (string)action["type"];
                switch (type)
                {
                    case "Wait": // (time)
                        long time = (long)action["time"];
                        Logger.Info(this, string.Format("{0}等待 {1} 毫秒", indent, time));
                        break;
                    case "WaitColor": // (x, y, color)
                        long wait_x = (long)action["x"];
                        long wait_y = (long)action["y"];
                        string color = (string)action["color"];
                        Logger.Info(this, string.Format("{0}等待直到 ({1}, {2}) 出現顏色 {3}", indent, wait_x, wait_y, color));
                        break;
                    case "SetCursorPos": // (x, y)
                        long to_x = (long)action["x"];
                        long to_y = (long)action["y"];
                        Logger.Info(this, string.Format("{0}滑鼠移動到 ({1}, {2})", indent, to_x, to_y));
                        break;
                    case "LMBClick": // ()
                        Logger.Info(this, string.Format("{0}點一下左鍵", indent));
                        break;
                    case "PressKey": // (keys)
                        string keys = (string)action["keys"];
                        Logger.Info(this, string.Format("{0}按下按鍵 {1}", indent, keys));
                        break;
                    case "Repeat": // (count, actions)
                        long count = (long)action["count"];
                        Logger.Info(this, string.Format("{0}重複執行 {1} 次:", indent, count));
                        object obj = action["actions"];
                        ResolveActionTable(obj, depth + 1);
                        break;
                    default:
                        string exceptionMessage = string.Format("未處理的 action: {0}", type);
                        Logger.Error(this, exceptionMessage);
                        throw new Exception(exceptionMessage);
                }
            }
        }

        private Lua? LoadLua(string scriptPath)
        {
            if (openedLua.TryGetValue(scriptPath, out Lua? lua) && lua != null)
            {
                return lua;
            }
            return Open(scriptPath);
        }

        private Lua? Open(string scriptPath)
        {
            Lua lua = new Lua();
            lua.State.Encoding = Encoding.UTF8;
            try
            {
                lua.DoFile(scriptPath);
                Logger.Info(this, string.Format("Lua文件解析成功: {0}", Path.GetFileName(scriptPath)));
                openedLua.Add(scriptPath, lua);
                return lua;
            }
            catch (Exception e)
            {
                Logger.Error(this, string.Format("解析Lua文件({0})時遇上了錯誤: {1}", Path.GetFileName(scriptPath), e));
                return null;
            }
        }

        private void Close(string scriptPath)
        {
            if (openedLua.TryGetValue(scriptPath, out Lua? lua))
            {
                lua?.Dispose();
                openedLua.Remove(scriptPath);
            }
            Logger.Info(this, string.Format("已關閉Lua文件: {0}", Path.GetFileName(scriptPath)));
        }
    }
}
