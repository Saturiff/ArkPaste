using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace ArkWorker
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

        private void Click_B_End(object sender, RoutedEventArgs e) => FarmEnd();

        private void CB_BlackSlot_Checked(object sender, RoutedEventArgs e) => farm.enableBlackSlotCheck = true;

        private void CB_BlackSlot_Unchecked(object sender, RoutedEventArgs e) => farm.enableBlackSlotCheck = false;

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
            L_InfoText.Content = "背景推土啟用中";
        }

        private void FarmPause()
        {
            ToggleFarmButton(true);
            ShowMessage("背景推土已停止");
        }

        private void FarmEnd()
        {
            farm.FarmEnd();
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
    }
}
