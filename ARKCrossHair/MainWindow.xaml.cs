using System;
using System.Windows;
using System.Windows.Input;

namespace ARKCrossHair
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        CrosshairWindow crosshairW = new CrosshairWindow();

        private void ToggleCrosshair(bool isEnable)
        {
            if (isEnable) crosshairW.Show();
            else          crosshairW.Hide();
        }
        
        private void ClickMin(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void ClickExit(object sender, RoutedEventArgs e)
        {
            crosshairW.Close();
            Close();
            Environment.Exit(Environment.ExitCode);
        }

        private void ClickDrag(object sender, MouseButtonEventArgs e) => DragMove();

        private void ClickShowButton(object sender, MouseEventArgs e) => ShowButton(true);

        private void ClickHideButton(object sender, MouseEventArgs e) => ShowButton(false);

        private void ShowButton(bool show) => Min_button.Opacity = show ? 1 : 0;

        private void CB_Crosshair_Checked(object sender, RoutedEventArgs e) => ToggleCrosshair(true);

        private void CB_Crosshair_Unchecked(object sender, RoutedEventArgs e) => ToggleCrosshair(false);
    }
}
