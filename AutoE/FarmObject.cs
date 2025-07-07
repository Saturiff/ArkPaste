using System;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

using GregsStack.InputSimulatorStandard;
using GregsStack.InputSimulatorStandard.Native;

namespace AutoE
{
    internal class FarmObject
    {
        public FarmObject(IntPtr arkHandle) { this.arkHandle = arkHandle; }

        public Timer farmTimer; // 推土

        public int blackSoltCnt = 0;

        public const int blackSlotDelay = 300;

        private IntPtr arkHandle;

        public bool enableBlackSlotCheck = false;

        public VirtualKeyCode targetKeyCode = VirtualKeyCode.VK_E;

        public int interval = 500;

        public void FarmStart()
        {
            farmTimer = new Timer { Interval = interval };
            farmTimer.Tick += new EventHandler(FarmTimerTick);
            farmTimer.Start();
        }

        public void FarmPause()
        {
            farmTimer.Stop();
        }

        private InputSimulator sim = new InputSimulator();

        private static void PressKey(InputSimulator sim, VirtualKeyCode key, bool delay = false)
        {
            if (delay)
            {
                sim.Keyboard.KeyDown(key);
                sim.Keyboard.Sleep(100);
                sim.Keyboard.KeyUp(key);

                return;
            }

            sim.Keyboard.KeyPress(key);
        }

        private void FarmTimerTick(object sender, EventArgs e)
        {
            PressKey(sim, targetKeyCode);
        }

        [DllImport("user32")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}