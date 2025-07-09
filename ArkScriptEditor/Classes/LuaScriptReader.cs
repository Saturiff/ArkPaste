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
                Logger.Error(this, "無法讀取全局延遲 (腳本異常)，使用預設的 16 毫秒");
                return 16;
            }

            double value = (double)lua["GlobalDelay"];

            return Math.Max((int)value, 1);
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

        public List<IScriptAction> LoadScriptActions(string scriptPath, out string actionDump)
        {
            Logger.Info(this, string.Format("開始讀取 {0} 的動作清單...", Path.GetFileName(scriptPath)));

            Lua? lua = LoadLua(scriptPath);
            if (lua == null)
            {
                actionDump = "無法讀取 Run 方法 (腳本異常)";
                Logger.Error(this, actionDump);
                return [];
            }

            LuaFunction runFunc = (LuaFunction)lua["Run"];
            if (runFunc == null)
            {
                actionDump = "無法讀取 Run 方法 (請檢查方法是否存在與合法)";
                Logger.Error(this, actionDump);
                return [];
            }

            object[] runData = runFunc.Call();
            (List<IScriptAction> actions, actionDump) = ResolveActionTable(runData[0]);
            Logger.Info(this, "讀取結束\n");
            return actions;
        }

        private (List<IScriptAction>, string) ResolveActionTable(object tableObject, int depth = 0)
        {
            string indent = new string(' ', depth * 4);
            string actionDump = "";

            LuaTable actionTables = (LuaTable)tableObject;
            if (actionTables.Values.Count == 0)
            {
                actionDump += string.Format("{0}這是一個空的動作清單\n", indent);
                return ([], actionDump);
            }

            List<IScriptAction> actions = [];
            foreach (LuaTable actionTable in actionTables.Values)
            {
                string type = (string)actionTable["type"];
                switch (type)
                {
                    case "Wait": // (time)
                        long time = (long)actionTable["time"];
                        actions.Add(new Action_Wait(time));
                        actionDump += string.Format("{0}等待 {1} 毫秒\n", indent, time);
                        break;
                    case "WaitColor": // (x, y, r, g, b)
                        long wait_x = (long)actionTable["x"];
                        long wait_y = (long)actionTable["y"];
                        long r = (long)actionTable["r"];
                        long g = (long)actionTable["g"];
                        long b = (long)actionTable["b"];
                        actions.Add(new Action_WaitColor(wait_x, wait_y, r, g, b));
                        actionDump += string.Format("{0}等待直到 ({1}, {2}) 出現顏色 ({3}, {4}, {5})\n", indent, wait_x, wait_y, r, g, b);
                        break;
                    case "SetCursorPos": // (x, y)
                        long to_x = (long)actionTable["x"];
                        long to_y = (long)actionTable["y"];
                        actions.Add(new Action_SetCursorPos(to_x, to_y));
                        actionDump += string.Format("{0}滑鼠移動到 ({1}, {2})\n", indent, to_x, to_y);
                        break;
                    case "LMBClick": // ()
                        actions.Add(new Action_LMBClick());
                        actionDump += string.Format("{0}點一下左鍵\n", indent);
                        break;
                    case "PressKey": // (keys)
                        string keys = (string)actionTable["keys"];
                        actions.Add(new Action_PressKey(keys));
                        actionDump += string.Format("{0}按下按鍵 {1}\n", indent, keys);
                        break;
                    case "Repeat": // (count, actions)
                        long count = (long)actionTable["count"];
                        actionDump += string.Format("{0}重複執行 {1} 次:\n", indent, count);
                        
                        object obj = actionTable["actions"];
                        (var outActions, var outActionDump) = ResolveActionTable(obj, depth + 1);
                        actions.Add(new Action_Repeat(count, outActions));
                        actionDump += outActionDump;
                        break;
                    default:
                        string s = string.Format("未處理的 action: {0} 請檢查腳本", type);
                        actionDump += "!! " + s + Environment.NewLine;
                        string exceptionMessage = string.Format("未處理的 action: {0} 請檢查腳本", type);
                        Logger.Error(this, exceptionMessage);
                        break;
                }
            }
            return (actions, actionDump);
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
