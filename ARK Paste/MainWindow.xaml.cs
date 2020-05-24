using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Threading;
using Point = System.Drawing.Point;
using Timer = System.Windows.Forms.Timer;

namespace ARK_Paste
{
    public partial class MainWindow : Window
    {
        IntPtr ArkHandle = FindWindow(null, "ARK: Survival Evolved");

        #region Main
        public MainWindow()
        {
            InitializeComponent();
            InitFarm();
            ResizeMode = ResizeMode.CanMinimize;
        }
        #endregion

        #region 推土
        private void Click_B_farm_start(object sender, RoutedEventArgs e) => FarmStart();

        private void Click_B_farm_stop(object sender, RoutedEventArgs e) => FarmPause();

        private void Click_B_end(object sender, RoutedEventArgs e) => Environment.Exit(0);

        FarmObject farm = new FarmObject();

        private void InitFarm() => ToggleFarmButton(true);

        private void ToggleFarmButton(bool enable)
        {
            B_start_farm.IsEnabled = enable;
            B_pause_farm.IsEnabled = !enable;
        }

        private void FarmStart()
        {
            ToggleFarmButton(false);
            farm.FarmTimer = new Timer { Interval = 100 };
            farm.FarmTimer.Tick += new EventHandler(FarmTimerTick);
            farm.FarmTimer.Start();
        }

        private void FarmPause()
        {
            ToggleFarmButton(true);
            farm.FarmTimer.Stop();
            Lable_info.Content = "背景推土已停止";
        }

        private void FarmTimerTick(object sender, EventArgs e)
        {
            const int WM_LBUTTONDOWN = 0x0201;
            const int WM_LBUTTONUP = 0x0202;
            PostMessage(ArkHandle, WM_LBUTTONDOWN, 1, 0);
            PostMessage(ArkHandle, WM_LBUTTONUP, 0, 0);

            Lable_info.Content = "背景推土啟用中";

            //黑格偵測
            /*
            Point black_solt_location = new Point(1835, 35); // reina
            //System.Drawing.Point black_solt_location = new System.Drawing.Point(1822, 40); // dis

            if (GetColorAt(black_solt_location).R == 0 &&
                GetColorAt(black_solt_location).G == 0 &&
                GetColorAt(black_solt_location).B == 0)
            {
                farm.blackSoltCnt++;
            }

            if (farm.blackSoltCnt > 10)
            {
                farm.FarmTimer.Stop();
                farm.blackSoltCnt = 0;
                Lable_info.Content = "黑格，請丟棄物品";
                Black_slot();
            }
            */
        }
        /*
        private void Black_slot()
        {
            Lable_info.Content = "黑格，正在丟棄...";
            PostMessage(ArkHandle, 256, (int)Keys.F, 1); //開背包
            System.Threading.Thread.Sleep(1000);

            SearchAndDrop(Keys.B);
            SearchAndDrop(Keys.R);
            SearchAndDrop(Keys.C);
            SearchAndDrop(Keys.O);
            SearchAndDrop(Keys.F);

            PostMessage(ArkHandle, 256, (int)Keys.F, 1);
            System.Threading.Thread.Sleep(1000);
            Lable_info.Content = "背景推土啟用中";
            farm.FarmTimer.Start();
        }
        */
        #endregion

        #region 常用操作
        class PlayerParameter
        {
            public PlayerParameter() { }
            public const int searchBoxX = 1333, searchBoxY = 200;
            //public const int transferBoxX = 1444, transferBoxY = 200;
            public const int dropBoxX = 1490, dropBoxY = 200;
            public const int meatX = 326, meatY = 303;
            //public const int meatR = 152, meatG = 57, meatB = 55;
        }

        void LMBClick()
        {
            const int WM_LBUTTONDOWN = 0x0201;
            const int WM_LBUTTONUP = 0x0202;
            PostMessage(ArkHandle, WM_LBUTTONDOWN, 1, 0);
            PostMessage(ArkHandle, WM_LBUTTONUP, 0, 0);
        }

        void Wait(int t = FarmObject.blackSlotDelay) => Thread.Sleep(t);

        //void SearchAndTransfer(Keys[] key)
        //{
        //    SetCursorPos(searchBoxX, searchBoxY);
        //    Wait(blackSlotDelay);
        //    LMBClick();
        //    Wait(blackSlotDelay);
        //    for (int i = 0; i < key.Length; i++)
        //    {
        //        PostMessage(ArkHandle, 256, (int)key[i], 1);
        //        Wait(blackSlotDelay);
        //    }
        //    SetCursorPos(transferBoxX, transferBoxY);
        //    LMBClick();
        //    Wait(blackSlotDelay);
        //    mouse_event(0x8000 | 0x0001,
        //                Paste.screen2dx,
        //                Paste.screen2dy, 0, 0); //move
        //    Wait(50);
        //    PostMessage(ArkHandle, 256, (int)Keys.T, 1);
        //}
        /*
        void SearchAndDrop(Keys key)
        {
            SetCursorPos(PlayerParameter.searchBoxX, PlayerParameter.searchBoxY);
            Wait(FarmObject.blackSlotDelay);
            LMBClick();
            Wait(FarmObject.blackSlotDelay);
            PostMessage(ArkHandle, 256, (int)key, 1);
            Wait(FarmObject.blackSlotDelay);
            SetCursorPos(PlayerParameter.dropBoxX, PlayerParameter.dropBoxY);
            LMBClick();
            Wait(FarmObject.blackSlotDelay);
        }
        */
        #endregion
        #region DLL
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y,
            int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        [DllImport("user32.dll")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        const int MOUSEEVENTF_MOVE = 0x0001; //MOVE
        const int MOUSEEVENTF_LEFTDOWN = 0x0002; //LD 
        const int MOUSEEVENTF_LEFTUP = 0x0004; //LU
        const int MOUSEEVENTF_ABSOLUTE = 0x8000; //ABSOLUTE

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        // Get a handle to an application window.
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        // Activate an application window.
        //[DllImport("user32.dll")]
        //public static extern bool SetForegroundWindow(IntPtr hWnd);

        // Mouse move
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);
        #endregion

    }
}
