using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Timer = System.Windows.Forms.Timer;

namespace ArkPaste
{
    class FarmObject
    {
        public FarmObject(IntPtr arkHandle) { this.arkHandle = arkHandle; }

        public Timer farmTimer; // 推土

        public int blackSoltCnt = 0;

        public const int blackSlotDelay = 300;

        private readonly object screenPixel;

        private readonly object farm;

        private IntPtr arkHandle;

        public bool enableBlackSlotCheck = false;

        public void FarmStart()
        {
            farmTimer = new Timer { Interval = 100 };
            farmTimer.Tick += new EventHandler(farmTimerTick);
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

        private void farmTimerTick(object sender, EventArgs e)
        {
            PostMessage(arkHandle, WM_LBUTTONDOWN, 1, 0);
            PostMessage(arkHandle, WM_LBUTTONUP, 0, 0);
            
            //黑格偵測
            if (enableBlackSlotCheck)
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

            }
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

        private void BlackSlot()
        {
            PostMessage(arkHandle, 256, (int)Keys.F, 1); // 開背包
            Thread.Sleep(1000);

            SearchAndDrop(Keys.B);
            SearchAndDrop(Keys.R);
            SearchAndDrop(Keys.C);
            SearchAndDrop(Keys.O);
            SearchAndDrop(Keys.F);

            PostMessage(arkHandle, 256, (int)Keys.F, 1); // 關背包
            Thread.Sleep(1000);

            farmTimer.Start();
        }


        void LMBClick()
        {
            PostMessage(arkHandle, WM_LBUTTONDOWN, 1, 0);
            PostMessage(arkHandle, WM_LBUTTONUP, 0, 0);
        }

        void Wait(int blackSlotDelay) => Thread.Sleep(blackSlotDelay);

        void SearchAndDrop(Keys key)
        {
            SetCursorPos(searchBoxX, searchBoxY);
            Wait(blackSlotDelay);
            LMBClick();
            Wait(blackSlotDelay);
            PostMessage(arkHandle, 256, (int)key, 1);
            Wait(blackSlotDelay);
            SetCursorPos(dropBoxX, dropBoxY);
            LMBClick();
            Wait(blackSlotDelay);
        }

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;

        [DllImport("user32")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("gdi32", SetLastError = true)]
        private static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);


        public const int searchBoxX = 1333, searchBoxY = 200;
        public const int dropBoxX = 1490, dropBoxY = 200;
    }
}
