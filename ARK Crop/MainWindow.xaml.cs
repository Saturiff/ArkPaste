using ARK_Crop.Classes;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Timer = System.Windows.Forms.Timer;

namespace ARK_Crop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitHarvest();
            ResizeMode = ResizeMode.CanMinimize;
        }

        IntPtr ArkHandle = FindWindow(null, "ARK: Survival Evolved");

        #region 按鈕

        private void Click_B_start(object sender, RoutedEventArgs e) => HarvestStart();
        private void Click_B_set(object sender, RoutedEventArgs e) => HarvestSet();
        private void Click_B_stop(object sender, RoutedEventArgs e) => HarvestPause();
        private void Click_B_end(object sender, RoutedEventArgs e) => HarvestEnd();
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            item.isPaste = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            item.isPaste = false;
        }
        #endregion

        #region 採集
        HarvestObject item;
        //          | start | pause |  set  |  stop |
        // ------------------------------------------
        // Init     |   1       0       1       0
        // Starting |   0       1       0       0
        // Setting  |   0       0       1       1
        // Idle     |   1       0       1       1
        private enum HarvestStatus
        {
            Init, Starting, Setting, Idle
        }

        private void InitHarvest()
        {
            ToggleHarvestButton(HarvestStatus.Init, ButtonStatus.NormalMode);
            item = new HarvestObject(ArkHandle);
            InitColorPicker();
            if (Properties.Settings.Default.M_Box_item_position_x != 0) //load memory if not empty
            {
                LoadDataFromSetting();

                SetStatus("初始化完成!\n已找到並同步設定檔"
                    + "\nX = " + Properties.Settings.Default.M_Box_item_position_x
                    + "\tY = " + Properties.Settings.Default.M_Box_item_position_y);
            }
            else
            {
                SetStatus("初始化完成!\n未找到設定檔，請設定水泥座標");
                B_start.IsEnabled = false;
            }
        }
        private void ToggleHarvestButton(HarvestStatus hStat, ButtonStatus bStat)
        {
            bool start = false, pause = false, set = false, stop = false;
            if (hStat == HarvestStatus.Init)
            {
                start = true; pause = false; set = true; stop = false;
            }
            else if (hStat == HarvestStatus.Starting)
            {
                start = false; pause = true; set = false; stop = false;
            }
            else if (hStat == HarvestStatus.Setting)
            {
                start = false; pause = false; set = true; stop = true;
            }
            else if (hStat == HarvestStatus.Idle)
            {
                start = true; pause = false; set = true; stop = true;
            }
            B_start.IsEnabled = start;
            B_pause.IsEnabled = pause;
            B_set.IsEnabled = set;
            B_end.IsEnabled = stop;
            if (bStat == ButtonStatus.NormalMode)
            {
                B_set.Content = "設定座標";
                B_end.Content = "結束";
            }
            else if (bStat == ButtonStatus.StorageMode)
            {
                B_set.Content = "儲存座標";
                B_end.Content = "取消變更";
            }
        }

        private void LoadDataFromSetting()
        {
            B_start.IsEnabled = true;
            item.location.X = Properties.Settings.Default.M_Box_item_position_x;
            item.location.Y = Properties.Settings.Default.M_Box_item_position_y;
            item.screen2dx = item.location.X * 65535 / SystemInformation.PrimaryMonitorSize.Width;
            item.screen2dy = item.location.Y * 65535 / SystemInformation.PrimaryMonitorSize.Height;
            item.itemR = Properties.Settings.Default.M_Color_itemR;
            item.itemG = Properties.Settings.Default.M_Color_itemG;
            item.itemB = Properties.Settings.Default.M_Color_itemB;
        }

        private void HarvestStart()
        {
            if (ArkHandle != IntPtr.Zero)
            {
                ToggleHarvestButton(HarvestStatus.Starting, ButtonStatus.NormalMode);
                item.InitAutoHarvest();
            }
            else SetStatus("未偵測到方舟");
        }

        private void HarvestPause()
        {
            SetStatus("暫停");
            item.HarvestTimer.Stop();
            ToggleHarvestButton(HarvestStatus.Idle, ButtonStatus.NormalMode);
        }

        private enum ButtonStatus
        {
            NormalMode, StorageMode
        }
        ButtonStatus currentButtonStatus = ButtonStatus.NormalMode;

        private void HarvestSet()
        {
            if (currentButtonStatus == ButtonStatus.NormalMode) currentButtonStatus = ButtonStatus.StorageMode;
            else if (currentButtonStatus == ButtonStatus.StorageMode) currentButtonStatus = ButtonStatus.NormalMode;

            if (currentButtonStatus == ButtonStatus.NormalMode)
            {
                SetStatus("設定完成！");
                ToggleHarvestButton(HarvestStatus.Idle, ButtonStatus.NormalMode);
            }
            else if (currentButtonStatus == ButtonStatus.StorageMode)
            {
                SetStatus("等待新座標..");
                ToggleHarvestButton(HarvestStatus.Setting, ButtonStatus.StorageMode);
                ClickArea clickArea = new ClickArea(item, Box_item_position_x, Box_item_position_y);
                clickArea.Show();
            }
        }

        private void HarvestEnd()
        {
            // 按鈕狀態: 結束軟體 / 取消設定
            if (currentButtonStatus == ButtonStatus.NormalMode)
            {
                Close();
                Environment.Exit(Environment.ExitCode);
            }
            else if (currentButtonStatus == ButtonStatus.StorageMode)
            {
                SetStatus("設定完成！");
                ToggleHarvestButton(HarvestStatus.Idle, ButtonStatus.NormalMode);
            }
        }

        #endregion

        #region 雜項

        private void SetStatus(string msg)
        {
            Lable_status.Dispatcher.Invoke(() => Lable_status.Content = msg);
        }

        #endregion

        #region 顏色選取

        private void InitColorPicker()
        {
            item.PickColorTimer = new Timer { Interval = 2000 };
            item.PickColorTimer.Tick += new EventHandler(PickColorTimerTick);
            item.PickColorTimer.Start();
        }

        private void PickColorTimerTick(object sender, EventArgs e)
        {
            Point point = Control.MousePosition;
            new Point(point.X, point.Y);
        }

        #endregion

        #region DLL

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        #endregion

    }
}
