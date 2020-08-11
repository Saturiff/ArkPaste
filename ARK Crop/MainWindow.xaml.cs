using ArkHarvester.Classes;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Timer = System.Windows.Forms.Timer;

namespace ArkHarvester
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            InitHarvest();
        }

        IntPtr ArkHandle = FindWindow(null, "ARK: Survival Evolved");

        #region 按鈕

        private void ClickStart(object sender, RoutedEventArgs e) => HarvestStart();
        private void ClickSet(object sender, RoutedEventArgs e) => HarvestSet();
        private void ClickStop(object sender, RoutedEventArgs e) => HarvestPause();
        private void ClickEnd(object sender, RoutedEventArgs e) => HarvestEnd();

        private void CheckBoxChecked(object sender, RoutedEventArgs e) => item.isPaste = true;
        private void CheckBoxUnchecked(object sender, RoutedEventArgs e) => item.isPaste = false;

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

        private enum ButtonStatus
        {
            NormalMode, StorageMode
        }

        private ButtonStatus currentButtonStatus = ButtonStatus.NormalMode;

        // 初始化按鈕狀態、顏色偵測器、物品欄位資料(位置與顏色)
        private void InitHarvest()
        {
            ToggleHarvestButton(HarvestStatus.Init, ButtonStatus.NormalMode);

            item = new HarvestObject(ArkHandle);

            InitColorPicker();

            SlotData sd = Properties.Settings.Default.slotData;
            if (sd != null && sd.isVaild) // load memory if not empty
            {
                LoadDataFromSlotData(sd);

                SetStatus("初始化完成!\n已找到並同步設定檔\nX = " + sd.x + "\tY = " + sd.y);
            }
            else
            {
                Properties.Settings.Default.slotData = new SlotData();

                B_Start.IsEnabled = false;

                SetStatus("初始化完成!\n未找到設定檔，請設定水泥座標");
            }
        }

        // 根據不同狀態切換按鈕是否啟用與顯示的文字
        private void ToggleHarvestButton(HarvestStatus hStat, ButtonStatus bStat)
        {
            bool start = false, pause = false, set = false, stop = false;

            switch (hStat)
            {
                case HarvestStatus.Init:
                    start = true; pause = false; set = true; stop = false;
                    break;
                case HarvestStatus.Starting:
                    start = false; pause = true; set = false; stop = false;
                    break;
                case HarvestStatus.Setting:
                    start = false; pause = false; set = true; stop = true;
                    break;
                case HarvestStatus.Idle:
                    start = true; pause = false; set = true; stop = true;
                    break;
            }

            B_Start.IsEnabled = start;
            B_pause.IsEnabled = pause;
            B_Set.IsEnabled = set;
            B_end.IsEnabled = stop;

            if (bStat == ButtonStatus.NormalMode)
            {
                B_Set.Content = "設定座標";
                B_end.Content = "結束";
            }
            else if (bStat == ButtonStatus.StorageMode)
            {
                B_Set.Content = "儲存座標";
                B_end.Content = "取消變更";
            }
        }

        private void LoadDataFromSlotData(SlotData sd)
        {
            B_Start.IsEnabled = true;

            item.UpdateFromSlotData(sd);
        }

        private void HarvestStart()
        {
            if (ArkHandle != IntPtr.Zero)
            {
                ToggleHarvestButton(HarvestStatus.Starting, ButtonStatus.NormalMode);

                item.InitAutoHarvest();
            }
            else
            {
                SetStatus("未偵測到方舟");
            }
        }

        private void HarvestPause()
        {
            SetStatus("暫停");

            item.HarvestTimer.Stop();

            ToggleHarvestButton(HarvestStatus.Idle, ButtonStatus.NormalMode);
        }

        // 設定座標
        private void HarvestSet()
        {
            // 切換狀態
            if (currentButtonStatus == ButtonStatus.NormalMode)
            {
                currentButtonStatus = ButtonStatus.StorageMode;
            }
            else if (currentButtonStatus == ButtonStatus.StorageMode)
            {
                currentButtonStatus = ButtonStatus.NormalMode;
            }

            if (currentButtonStatus == ButtonStatus.NormalMode)
            {
                SetStatus("設定完成！");
                ToggleHarvestButton(HarvestStatus.Idle, ButtonStatus.NormalMode);
            }
            else if (currentButtonStatus == ButtonStatus.StorageMode)
            {
                SetStatus("等待新座標..");
                ToggleHarvestButton(HarvestStatus.Setting, ButtonStatus.StorageMode);
                ClickArea clickArea = new ClickArea(item, Box_ItemPositionX, Box_ItemPositionY);
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
