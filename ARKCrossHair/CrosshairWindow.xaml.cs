using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ARKCrossHair
{
    public partial class CrosshairWindow : Window
    {
        public CrosshairWindow()
        {
            InitializeComponent();
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
            WindowStartupLocation = WindowStartupLocation.Manual;
            Opacity = 1.0;

            arkHandle = FindWindow(null, "ARK: Survival Evolved");

            InitCrosshair();
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (arkHandle != IntPtr.Zero)
            {
                RECT wrect;
                GetWindowRect(new HandleRef(this, arkHandle), out wrect);

                RECT crect;
                GetClientRect(new HandleRef(this, arkHandle), out crect);

                POINT lefttop; lefttop.X = crect.Left; lefttop.Y = crect.Top;
                ClientToScreen(new HandleRef(this, arkHandle), ref lefttop);

                POINT rightbottom; rightbottom.X = crect.Right; rightbottom.Y = crect.Bottom;
                ClientToScreen(new HandleRef(this, arkHandle), ref rightbottom);

                int leftBorder = lefttop.X - wrect.Left;            // Windows 10: includes transparent part
                // int rightBorder = wrect.Right - rightbottom.X;   // As above
                // int bottomBorder = wrect.Bottom - rightbottom.Y; // As above
                int topBorderWithTitleBar = lefttop.Y - wrect.Top;  // There is no transparent part

                Left = wrect.Left + leftBorder;
                Top = wrect.Top + topBorderWithTitleBar;
                Width = crect.Right;
                Height = crect.Bottom;
            }
            else
            {
                Left = 0;
                Top = 0;
                Width = 1920;
                Height = 1080;
            }
        }

        private IntPtr arkHandle;
        protected override void OnSourceInitialized(EventArgs e)
        {
            WindowsServices.SetWindowExTransparent(new WindowInteropHelper(this).Handle);
        }

        private void InitCrosshair()
        {
            BitmapImage bitmapImg = new BitmapImage();
            bitmapImg.BeginInit();
            bitmapImg.UriSource = new Uri("/img/crosshair.png", UriKind.Relative);
            bitmapImg.DecodePixelWidth = 1000;
            bitmapImg.EndInit();
            Img_Crosshair.Source = bitmapImg;
        }

        #region DLL

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetClientRect(HandleRef hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(HandleRef hwnd, ref POINT lpPoint);

        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }
        [DllImport("user32.dll")]

        private static extern IntPtr FindWindow(string className, string windowName);

        #endregion

    }
}
