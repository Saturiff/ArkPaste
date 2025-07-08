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
            foreach (string scriptPath in openedLua.Keys)
            {
                Close(scriptPath);
            }
        }

        private readonly Dictionary<string, Lua> openedLua = [];

        public string LoadScriptDescription(string scriptPath)
        {
            Lua? lua = LoadLua(scriptPath);
            if (lua == null)
            {
                Logger.Error(this, "無法讀取描述字串 (Lua)");
                return "";
            }

            LuaFunction func = (LuaFunction)lua["Description"];
            if (func == null)
            {
                Logger.Error(this, "無法讀取描述字串 (func)");
                return "";
            }

            object[] ret = func.Call([]);
            if (ret == null)
            {
                Logger.Error(this, "無法讀取描述字串: impl請實現 Description 方法\n範例:\function Description()\r    return \"將此替換成描述文字\"\nend");
                return "";
            }

            return (string)ret[0];
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
            Logger.Info(this, string.Format("已關閉Lua文件: {0}", scriptPath));
        }
    }
}
