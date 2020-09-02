using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace ArkWorker
{
    public class HarvestObject : ItemLocation
    {
        public HarvestObject(IntPtr arkHandle) { this.arkHandle = arkHandle; }
        private IntPtr arkHandle = IntPtr.Zero;
        public Timer HarvestTimer, PickColorTimer;
        public int screen2dx = 0, screen2dy = 0;
        public int itemR = 0, itemG = 0, itemB = 0;
        public bool isPaste = true;

        public void InitAutoHarvest()
        {
            HarvestTimer = new Timer { Interval = 100 };
            HarvestTimer.Tick += new EventHandler(HarvestTimerTick);
            HarvestTimer.Start();
        }
        private void PasteTimerTick(object sender, EventArgs e)
        {
            bool isPicking = false;
            if (Properties.Settings.Default.slotData.x != 0)
            {
                if (isPaste)
                {
                    mouse_event(0x8000 | 0x0001,
                                    screen2dx,
                                    screen2dy, 0, 0); //move
                    PostMessage(arkHandle, 256, (int)Keys.T, 1);
                    PostMessage(arkHandle, 256, (int)Keys.F, 1);
                }
                else
                {
                    while (IsItemDetected())
                    {
                        isPicking = true;
                        mouse_event(0x8000 | 0x0001,
                                    screen2dx,
                                    screen2dy, 0, 0); //move
                        Thread.Sleep(10);
                        PostMessage(arkHandle, 256, (int)Keys.T, 1);
                    }
                    if (isPicking) PostMessage(arkHandle, 256, (int)Keys.F, 1);
                }
            }
        }
        private void HarvestTimerTick(object sender, EventArgs e)
        {
            bool isPicking = false;
            if (Properties.Settings.Default.slotData.x != 0)
            {
                while (IsItemDetected())
                {
                    isPicking = true;
                    mouse_event(0x8000 | 0x0001,
                                screen2dx,
                                screen2dy, 0, 0); //move
                    Thread.Sleep(10);
                    PostMessage(arkHandle, 256, (int)Keys.T, 1);
                }
                if (isPicking) PostMessage(arkHandle, 256, (int)Keys.F, 1);
            }
        }
        private bool IsItemDetected()
        {
            return GetColorAt(location).R == itemR
                    && GetColorAt(location).G == itemG
                    && GetColorAt(location).B == itemB;
        }


        [DllImport("user32.dll")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        // const int MOUSEEVENTF_MOVE = 0x0001;     // MOVE
        // const int MOUSEEVENTF_LEFTDOWN = 0x0002; // LD 
        // const int MOUSEEVENTF_LEFTUP = 0x0004;   // LU
        // const int MOUSEEVENTF_ABSOLUTE = 0x8000; // ABSOLUTE

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
    }
}
