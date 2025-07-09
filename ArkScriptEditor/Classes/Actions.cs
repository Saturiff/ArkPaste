using System.Drawing;
using System.Windows.Forms;
using Point = System.Drawing.Point;

namespace ArkScriptEditor.Classes
{
    // TODO: And, Or, Not

    internal class Action_Wait(long time) : IScriptAction
    {
        private readonly long time = time;

        // runtime
        private long startTime = -1;

        public bool Execute(ScriptRunner ctx)
        {
            if (startTime == -1)
            {
                startTime = ctx.DeltaMillisecond;
            }

            bool result = (ctx.DeltaMillisecond - startTime) >= time;
            if (result)
            {
                // runtime reset
                startTime = -1;
                return true;
            }
            return false;
        }
    }

    internal class Action_WaitColor(long x, long y, long r, long g, long b) : IScriptAction
    {
        private readonly Point p = new Point((int)x, (int)y);
        private readonly Color c = Color.FromArgb((int)r, (int)g, (int)b);

        public bool Execute(ScriptRunner ctx)
        {
            return ScriptLibrary.IsColorAt(p, c);
        }
    }


    internal class Action_SetCursorPos(long x, long y) : IScriptAction
    {
        private readonly int posX = (int)x;
        private readonly int posY = (int)y;

        public bool Execute(ScriptRunner ctx)
        {
            ScriptLibrary.SetCursorPos(posX, posY);
            return true;
        }
    }

    internal class Action_LMBClick() : IScriptAction
    {
        public bool Execute(ScriptRunner ctx)
        {
            ScriptLibrary.LMBClick(ctx.HWnd);
            return true;
        }
    }

    internal class Action_PressKey : IScriptAction
    {
        public Action_PressKey(string keys)
        {
            string s = keys.ToUpper();

            keysArr = new Keys[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (Enum.TryParse(c.ToString(), out Keys outKey))
                {
                    keysArr[i] = outKey;
                    Logger.Info(this, string.Format("Got '{0}' from ch '{1}'", outKey, c));
                }
                else
                {
                    keysArr[i] = Keys.None;
                    Logger.Error(this, string.Format("初始化 PressKey 時轉換錯誤: 字元 '{1}' 無法轉換成有效的枚舉 (Got '{0}' from ch '{1}')", outKey, c));
                }
            }

            currentIndex = 0;
        }

        private readonly Keys[] keysArr;

        // runtime
        private int currentIndex;

        public bool Execute(ScriptRunner ctx)
        {
            if (currentIndex < keysArr.Length)
            {
                ScriptLibrary.PressKey(ctx.HWnd, keysArr[currentIndex++]);
                return false;
            }

            // runtime reset
            currentIndex = 0;
            return true;
        }
    }

    internal class Action_Repeat(long count, List<IScriptAction> actions) : IScriptAction
    {
        // runtime
        private int currentCount;
        private int currentIndex;

        public bool Execute(ScriptRunner ctx)
        {
            if (currentCount < count)
            {
                if (currentIndex < actions.Count)
                {
                    if (actions[currentIndex].Execute(ctx))
                    {
                        currentIndex++;
                    }
                    return false;
                }
                else
                {
                    currentIndex = 0;
                    currentCount++;
                }
            }

            if (currentCount >= count)
            {
                // runtime reset
                currentCount = 0;
                currentIndex = 0;
                return true;
            }
            return false;
        }
    }

    interface IScriptAction
    {
        bool Execute(ScriptRunner ctx);
    }
}
