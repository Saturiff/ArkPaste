﻿using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace ArkHarvester.Classes
{
    public class HarvestObject : ItemLocation
    {
        public HarvestObject(IntPtr arkHandle) { this.arkHandle = arkHandle; }
        private IntPtr arkHandle = IntPtr.Zero;
        public Timer HarvestTimer, PickColorTimer;
        public int screen2dx = 0, screen2dy = 0;
        public int itemR = 0, itemG = 0, itemB = 0;
        public bool isPaste = true;
        private SlotData sd;

        public void InitAutoHarvest()
        {
            HarvestTimer = new Timer { Interval = 100 };
            HarvestTimer.Tick += new EventHandler(HarvestTimerTick);
            HarvestTimer.Start();
        }

        public void UpdateFromSlotData(SlotData sd)
        {
            this.sd = sd;
            location.X = this.sd.x;
            location.Y = this.sd.y;
            screen2dx = this.sd.x * 65535 / SystemInformation.PrimaryMonitorSize.Width;
            screen2dy = this.sd.y * 65535 / SystemInformation.PrimaryMonitorSize.Height;
            itemR = this.sd.r;
            itemG = this.sd.g;
            itemB = this.sd.b;
        }

        private void PasteTimerTick(object sender, EventArgs e)
        {
            bool isPicking = false;
            if (sd.isVaild)
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
            if (!sd.isVaild)
            {
                return;
            }

            HarvestImpl_Maker(sender, e);
        }

        private void HarvestImpl_Maker(object sender, EventArgs e)
        {
            if (IsItemDetected())
            {
                HarvestTimer.Stop();
                mouse_event(0x8000 | 0x0001,
                            screen2dx,
                            screen2dy, 0, 0); //move
                Thread.Sleep(10);

                for (int i = 0; i < 10; i++)
                {
                    PostMessage(arkHandle, 256, (int)Keys.A, 1);
                    Thread.Sleep(50);
                }
                PostMessage(arkHandle, 256, (int)Keys.F, 1);
                HarvestTimer.Start();
            }
        }

        private void HarvestImpl_Paste(object sender, EventArgs e)
        {
            bool isPicking = false;
            if (sd.isVaild)
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
            //return GetColorAt(location).R == itemR
            //        && GetColorAt(location).G == itemG
            //        && GetColorAt(location).B == itemB;

            return Math.Abs(GetColorAt(location).R - itemR) < 5
                    && Math.Abs(GetColorAt(location).G - itemG) < 5
                    && Math.Abs(GetColorAt(location).B - itemB) < 5;
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
