using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Point = System.Drawing.Point;

namespace ArkScriptEditor.Classes
{
    internal static class ScriptLibrary
    {
        public static Color GetColorAt(Point location)
        {
            Bitmap p = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            try
            {
                using Graphics gdest = Graphics.FromImage(p);
                using Graphics gsrc = Graphics.FromHwnd(nint.Zero);
                nint hSrcDC = gsrc.GetHdc();
                nint hDC = gdest.GetHdc();
                int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                gdest.ReleaseHdc();
                gsrc.ReleaseHdc();
            }
            catch
            {
                Logger.Error("GetColorAt", "獲取顏色時發生了問題");
                throw;
            }

            return p.GetPixel(0, 0);
        }

        public static bool IsColorAt(Point location, Color c, int bias = 5)
        {
            Color _c = GetColorAt(location);

            // 給予一點容錯值
            return Math.Abs(_c.R - c.R) < bias
                && Math.Abs(_c.G - c.G) < bias
                && Math.Abs(_c.B - c.B) < bias;
        }

        public static void PressKey(nint hWnd, Keys key)
        {
            PostMessage(hWnd, 256, (int)key, 0);
        }

        public static void LMBClick(nint hWnd)
        {
            PostMessage(hWnd, WM_LBUTTONDOWN, 1, 0);
            PostMessage(hWnd, WM_LBUTTONUP, 0, 0);
        }

        public static void LMBClickAt(nint hWnd, ushort mouseX, ushort mouseY)
        {
            PostMessage(hWnd, WM_LBUTTONDOWN, 1, MAKELPARAM(mouseX, mouseY));
            PostMessage(hWnd, WM_LBUTTONUP, 0, MAKELPARAM(mouseX, mouseY));
        }

        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;

        public static int MAKELPARAM(int p, int p_2)
        {
            return p_2 << 16 | p & 0xFFFF;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32")]
        private static extern bool PostMessage(nint hWnd, uint Msg, int wParam, int lParam);

        [DllImport("gdi32", SetLastError = true)]
        private static extern int BitBlt(nint hDC, int x, int y, int nWidth, int nHeight, nint hSrcDC, int xSrc, int ySrc, int dwRop);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(nint hWnd);

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(nint hWnd, int id);
    }
}
