using GregsStack.InputSimulatorStandard.Native;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace AutoE
{
    public partial class MainWindow : Window
    {
        private IntPtr arkHandle = FindWindow(null, "ARK: Survival Evolved");

        #region Main
        public MainWindow()
        {
            InitializeComponent();
            InitFarm();
        }
        #endregion

        #region 推土
        private void Click_B_FarmStart(object sender, RoutedEventArgs e) => FarmStart();

        private void Click_B_FarmStop(object sender, RoutedEventArgs e) => FarmPause();

        private FarmObject farm;

        private void InitFarm()
        {
            ToggleFarmButton(true);
            farm = new FarmObject(arkHandle);
        }

        private void FarmStart()
        {
            ToggleFarmButton(false);
            farm.FarmStart();
            L_InfoText.Content = "啟用中";
        }

        private void FarmPause()
        {
            ToggleFarmButton(true);
            farm.FarmPause();
            ShowMessage("已停止");
        }

        private void ToggleFarmButton(bool enable)
        {
            B_StartFarm.IsEnabled = enable;
            B_PauseFarm.IsEnabled = !enable;
        }

        public void ShowMessage(string msg)
        {
            L_InfoText.Content = msg;
        }

        #endregion

        #region DLL

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        #endregion

        private void CB_ToA_Checked(object sender, RoutedEventArgs e)
        {
            farm.targetKeyCode = VirtualKeyCode.VK_A;
        }

        private void CB_ToA_Unchecked(object sender, RoutedEventArgs e)
        {
            farm.targetKeyCode = VirtualKeyCode.VK_E;
        }
    }
}
