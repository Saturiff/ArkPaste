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

        public object[]? LoadScriptRun(string scriptPath)
        {
            Lua? lua = LoadLua(scriptPath);
            if (lua == null)
            {
                Logger.Error(this, "無法讀取 Run 方法 (腳本異常)");
                return null;
            }

            LuaFunction runFunc = (LuaFunction)lua["Run"];
            if (runFunc == null)
            {
                Logger.Error(this, "無法讀取 Run 方法 (請檢查方法是否存在與合法)");
                return null;
            }

            object[] runData = runFunc.Call();

            LuaTable actions = (LuaTable)runData[0];
            foreach (LuaTable action in actions.Values)
            {
                string type = (string)action["type"];
                if (type == "Wait")
                {
                    long time = (long)action["time"];
                    Logger.Info(this, string.Format("等待 {0} 毫秒", time));
                }
                // TODO: dict <action type, action function>

            }
            return runData;
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
