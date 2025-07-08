using System.Drawing;
using System.Runtime.InteropServices;
using Point = System.Drawing.Point;
using Timer = System.Windows.Forms.Timer;

namespace ArkScriptEditor.Classes
{
    class ScriptRunner(nint arkHandle)
    {
        public bool enableBlackSlotCheck = false;

        public event EventHandler? FarmTimerTick;

        public void Start(int interval = 50)
        {
            if (farmTimer == null)
            {
                farmTimer = new Timer();
                farmTimer.Tick += OnTimerTick;
            }
            else
            {
                farmTimer.Interval = interval;
                farmTimer.Start();
            }
        }

        public void Stop()
        {
            if (farmTimer == null)
            {
                return;
            }
            farmTimer.Stop();
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            FarmTimerTick?.Invoke(sender, e);
            return;
        }

        private static Color GetColorAt(Point location)
        {
            Bitmap p = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics gdest = Graphics.FromImage(p))
            {
                using Graphics gsrc = Graphics.FromHwnd(nint.Zero);
                nint hSrcDC = gsrc.GetHdc();
                nint hDC = gdest.GetHdc();
                int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                gdest.ReleaseHdc();
                gsrc.ReleaseHdc();
            }

            return p.GetPixel(0, 0);
        }

        private static bool IsItemDetected(Point location, Color c)
        {
            Color gotC = GetColorAt(location);
            // 給予一點容錯值
            return Math.Abs(gotC.R - c.R) < 5
                    && Math.Abs(gotC.G - c.G) < 5
                    && Math.Abs(gotC.B - c.B) < 5;
        }

        private void BringToFront(nint hWnd)
        {
            SetForegroundWindow(hWnd);
        }

        void LMBClick()
        {
            PostMessage(arkHandle, WM_LBUTTONDOWN, 1, 0);
            PostMessage(arkHandle, WM_LBUTTONUP, 0, 0);
        }

        void LMBClickAt(ushort mouseX, ushort mouseY)
        {
            PostMessage(arkHandle, WM_LBUTTONDOWN, 1, MAKELPARAM(mouseX, mouseY));
            PostMessage(arkHandle, WM_LBUTTONUP, 0, MAKELPARAM(mouseX, mouseY));
        }

        private static void Wait(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }

        private Timer? farmTimer;

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;

        private static int MAKELPARAM(int p, int p_2)
        {
            return p_2 << 16 | p & 0xFFFF;
        }

        [DllImport("user32")]
        private static extern bool PostMessage(nint hWnd, uint Msg, int wParam, int lParam);

        [DllImport("gdi32", SetLastError = true)]
        private static extern int BitBlt(nint hDC, int x, int y, int nWidth, int nHeight, nint hSrcDC, int xSrc, int ySrc, int dwRop);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(nint hWnd);
    }
}
