using System;
using System.Runtime.InteropServices;

namespace ArkCrosshair
{
    public static class WindowsServices
    {
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int GWL_EXSTYLE = -20;
        /*
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);
        */
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        
        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_TRANSPARENT);
        }
    }
}
