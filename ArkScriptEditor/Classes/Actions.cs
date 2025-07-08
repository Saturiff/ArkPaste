namespace ArkScriptEditor.Classes
{
    internal class Action_Wait(long time) : IScriptAction
    {
        private readonly long time = time;

        public void Execcute()
        {
            throw new NotImplementedException();
        }
    }

    internal class Action_WaitColor(long x, long y, long r, long g, long b) : IScriptAction
    {
        private readonly long x = x;
        private readonly long y = y;
        private readonly long r = r;
        private readonly long g = g;
        private readonly long b = b;

        public void Execcute()
        {
            throw new NotImplementedException();
        }
    }


    internal class Action_SetCursorPos(long x, long y) : IScriptAction
    {
        private long x = x;
        private long y = y;

        public void Execcute()
        {
            throw new NotImplementedException();
        }
    }


    internal class Action_LMBClick() : IScriptAction
    {
        public void Execcute()
        {
            throw new NotImplementedException();
        }
    }

    internal class Action_PressKey(string keys) : IScriptAction
    {
        private string keys = keys;

        public void Execcute()
        {
            throw new NotImplementedException();
        }
    }

    internal class Action_Repeat(long count, List<IScriptAction> actions) : IScriptAction
    {
        private long count = count;
        List<IScriptAction> actions = actions;

        public void Execcute()
        {
            throw new NotImplementedException();
        }
    }

    interface IScriptAction
    {
        void Execcute();
    }
}
