using GregsStack.InputSimulatorStandard;
using GregsStack.InputSimulatorStandard.Native;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Timer = System.Windows.Forms.Timer;

namespace ArkWorker
{
    class FarmObject
    {
        public FarmObject(IntPtr arkHandle) { this.arkHandle = arkHandle; }

        public Timer farmTimer; // 推土

        public int blackSoltCnt = 0;

        public const int blackSlotDelay = 300;

        private IntPtr arkHandle;

        public bool enableBlackSlotCheck = false;

        public void FarmStart()
        {
            farmTimer = new Timer { Interval = 50 };
            farmTimer.Tick += new EventHandler(FarmTimerTick);
            farmTimer.Start();
        }

        public void FarmPause()
        {
            farmTimer.Stop();
        }

        public void FarmEnd()
        {
            Environment.Exit(0);
        }

        public void FarmDrop()
        {
            BlackSlot();
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


        private void DDDTick(object sender, EventArgs e)
        {
            PressKey(sim, VirtualKeyCode.VK_D, true);
        }

        ushort mouseX = 1000;
        ushort mouseY = 940;

        private void FarmTimerTick(object sender, EventArgs e)
        {
            Script_MakeAll();
            return;

            PostMessage(arkHandle, WM_LBUTTONDOWN, 1, MAKELPARAM(mouseX, mouseY));
            PostMessage(arkHandle, WM_LBUTTONUP, 0, MAKELPARAM(mouseX, mouseY));

            // PostMessage(arkHandle, WM_LBUTTONDOWN, 1, 0);
            // PostMessage(arkHandle, WM_LBUTTONUP, 0, 0);

            //黑格偵測
            /*if (enableBlackSlotCheck)
            {
                // TODO:
                // 加一個按鈕手動設定座標
                // 檢查設定中是否有已存在的座標資料
                // 如果有座標則直接執行
                // 如果沒有座標則開啟偵測座標介面準備儲存

                Point blackSlotLocation = new Point(1835, 35); // test

                if (GetColorAt(blackSlotLocation).R == 0 &&
                    GetColorAt(blackSlotLocation).G == 0 &&
                    GetColorAt(blackSlotLocation).B == 0)
                {
                    blackSoltCnt++;
                }

                if (blackSoltCnt > 10)
                {
                    farmTimer.Stop();
                    blackSoltCnt = 0;
                    BlackSlot();
                }

            }*/
        }

        private Color GetColorAt(Point location)
        {
            Bitmap p = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics gdest = Graphics.FromImage(p))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            return p.GetPixel(0, 0);
        }

        private bool IsItemDetected(Point location, Color c)
        {
            Color gotC = GetColorAt(location);
            //给予一点容许阈值
            return Math.Abs(gotC.R - c.R) < 5
                    && Math.Abs(gotC.G - c.G) < 5
                    && Math.Abs(gotC.B - c.B) < 5;
        }

        private Point watchingLocation = new Point(991, 164);
        private readonly Color waitingColor = Color.FromArgb(128, 231, 255);
        private void Script_MakeAll()
        {
            farmTimer.Stop();

            // wait color at pixel
            if (!IsItemDetected(watchingLocation, waitingColor))
            {
                farmTimer.Start();
                return;
            }

            Thread.Sleep(300);

            SetCursorPos(searchBoxX, searchBoxY);
            Wait(100);
            LMBClick();
            Wait(100);

            //Clipboard.SetText("火药");
            ////Clipboard.GetText();

            //PostMessage(arkHandle, 256, 0x11, 0);
            //PostMessage(arkHandle, 256, (int)Keys.V, 0);
            //PostMessage(arkHandle, 257, (int)Keys.V, 0);
            //PostMessage(arkHandle, 257, 0x11, 0);
            //Wait(100);

            //SetCursorPos(1370, 229); // 对方移动
            //Wait(100);

            //LMBClick();
            //Wait(100);



            Keys[] keys = new Keys[] { Keys.D, Keys.V };
            foreach (var key in keys)
            {
                PostMessage(arkHandle, 256, (int)key, 0);
                Wait(100);
            }
            SetCursorPos(1252, 308); // 第一格
            Wait(100);
            LMBClick();
            Wait(100);
            for (int i = 0; i < 10; i++)
            {
                PostMessage(arkHandle, 256, (int)Keys.A, 0);
                Wait(100);
            }

            PostMessage(arkHandle, 256, (int)Keys.F, 0); // 關背包
            Thread.Sleep(100);

            farmTimer.Start();
        }

        private void BlackSlot()
        {
            farmTimer.Stop();
            BringToFront(arkHandle);
            Thread.Sleep(1000);

            PostMessage(arkHandle, 256, (int)Keys.F, 0); // 開背包
            Thread.Sleep(1000);

            SearchAndDrop(Keys.B);
            SearchAndDrop(Keys.R);
            SearchAndDrop(Keys.C);
            SearchAndDrop(Keys.O);
            SearchAndDrop(Keys.F);

            PostMessage(arkHandle, 256, (int)Keys.F, 0); // 關背包
            Thread.Sleep(1000);

            farmTimer.Start();
        }

        private void BringToFront(IntPtr hWnd)
        {
            SetForegroundWindow(hWnd);
        }

        void LMBClick()
        {
            PostMessage(arkHandle, WM_LBUTTONDOWN, 1, 0);
            PostMessage(arkHandle, WM_LBUTTONUP, 0, 0);
        }

        void Wait(int millisecondsTimeout) => Thread.Sleep(millisecondsTimeout);

        void SearchAndDrop(Keys key)
        {
            SetCursorPos(searchBoxX, searchBoxY);
            Wait(blackSlotDelay);
            LMBClick();
            Wait(blackSlotDelay);
            PostMessage(arkHandle, 256, (int)key, 0);
            Wait(blackSlotDelay);
            SetCursorPos(dropBoxX, dropBoxY);
            LMBClick();
            Wait(blackSlotDelay);
        }

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;

        private int MAKELPARAM(int p, int p_2)
        {
            return (p_2 << 16) | (p & 0xFFFF);
        }

        [DllImport("user32")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("gdi32", SetLastError = true)]
        private static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        //public const int searchBoxX = 1333, searchBoxY = 185;
        public const int searchBoxX = 1235, searchBoxY = 225;
        public const int dropBoxX = 1475, dropBoxY = 185;
    }
}
