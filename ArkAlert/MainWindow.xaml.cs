/*
作者: 雷納 Reina
實現功能: 辨識螢幕文字，對已指定的微信群發出對應的警報訊息
環境需求: .Net framework 4.7.2, VC++ 轉發套件 x86
第三方需求: 微信手機版, 已加入通訊錄之微信群
註解最後編輯日期: 2019/08/23
*/
using System;
using System.Drawing;
using System.Windows;
using System.Threading;
using Timer = System.Windows.Forms.Timer;
using System.Collections.Generic;
using System.Diagnostics;
using WxBotDotNET;
using Tesseract;

namespace ArkAlert
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TB_Send.AcceptsReturn = true;
            CheckB_ParaWarning.IsEnabled = false;
            ResizeMode = ResizeMode.CanMinimize;
            if (Properties.Settings.Default.enemyMsg != string.Empty) TB_Send.Text = Properties.Settings.Default.enemyMsg;
            Trace.WriteLine("視窗初始化完成");
        }

        WxBot wxBot;
        private void InitWX()
        {
            Trace.WriteLine("");
            Trace.WriteLine("開始初始化微信");
            B_LoginWX.IsEnabled = false;
            wxBot = new WxBot();
            if (wxBot.Run()) foreach (wxContact group in wxBot.GetGroupList()) ComboB_GroupList.Items.Add(group.ShowName);
            else
            {
                B_LoginWX.IsEnabled = true;
                Lable_info.Content = "登入失敗，請重試\n若已重試多次，請通知原作者";
            }
            Trace.WriteLine("微信初始化完成");
            Trace.WriteLine("");
        }

        private Bitmap gBmp;
        private void InitBitmap() => gBmp = new Bitmap((int)SystemParameters.VirtualScreenWidth, 105);

        private enum WarningType { Enemy, ForestTitanStart, IceTitanStart, DesertTitanStart, TitanFinish }
        private Dictionary<WarningType, bool> warningDict = new Dictionary<WarningType, bool>()
        {
            { WarningType.Enemy, false },
            { WarningType.ForestTitanStart, false },
            { WarningType.IceTitanStart, false },
            { WarningType.DesertTitanStart, false },
            { WarningType.TitanFinish, false }
        };

        private bool isDetecting = false;
        private void DetectStatus()
        {
            Trace.WriteLine("新的偵測狀態");
            isDetecting = true;
            for (int scanIdx = 0; scanIdx < 4; scanIdx++)
            {
                Trace.WriteLine("偵測狀態: ");
                var bmp = new Bitmap(gBmp);
                if (scanIdx == 0) Trace.WriteLine("0. 敵人");
                else if (scanIdx == 1) Trace.WriteLine("1. 森林泰坦");
                else if (scanIdx == 2) Trace.WriteLine("2. 冰霜泰坦");
                else if (scanIdx == 3) Trace.WriteLine("3. 沙漠泰坦");

                bool isText = false;
                for (int w = 0; w < bmp.Width; w++)
                    for (int h = 0; h < bmp.Height; h++)
                    {
                        var pix = bmp.GetPixel(w, h);
                        if (scanIdx == 0) // 敵人
                        {
                            //if ((pix.R < 2) && (pix.G > 195) && (pix.B > 230)) isText = true;
                            if ((pix.R < 40) && (pix.G > 100) && (pix.B > 100)) isText = true;
                        }
                        else if (scanIdx == 1) // 森林
                        {
                            if ((pix.R < 20) && (pix.G > 100) && (pix.G < 200) && (pix.B < 20)) isText = true;
                        }
                        else if (scanIdx == 2) // 冰霜
                        {
                            if ((pix.R < 40) && (pix.G > 100) && (pix.B > 100)) isText = true;
                        }
                        else if (scanIdx == 3) // 沙漠
                        {
                            if ((pix.R > 100) && (pix.G > 100) && (pix.B < 30)) isText = true;
                        }

                        if (isText) bmp.SetPixel(w, h, Color.FromArgb(0, 0, 0));
                        else bmp.SetPixel(w, h, Color.FromArgb(255, 255, 255));
                        isText = false;
                    }
                Trace.WriteLine("啟動圖像辨識引擎");
                var ocr = new TesseractEngine("./tessdata", "eng", EngineMode.TesseractAndCube);
                Trace.WriteLine("圖像辨識引擎啟動完成");
                var scan = ocr.Process(bmp).GetText();
                Trace.WriteLine("回傳值 scan = " + scan);
                //bmp.Save("D:/Desktop/ark/ARKtess/out_" + scanIdx + ".png");
                //using (System.IO.StreamWriter sw = new System.IO.StreamWriter("D:/Desktop/ark/ARKtess/out_" + scanIdx + ".txt")) sw.WriteLine(scan);
                //bmp.Save("C:/Users/Reina/Desktop/ark/out_" + scanIdx + ".png");
                //using (System.IO.StreamWriter sw = new System.IO.StreamWriter("C:/Users/Reina/Desktop/ark/out_" + scanIdx + ".txt")) sw.WriteLine(scan);

                if (scan.Contains("started"))
                {
                    if (scanIdx == 1) warningDict[WarningType.ForestTitanStart] = true;
                    else if (scanIdx == 2) warningDict[WarningType.IceTitanStart] = true;
                    else if (scanIdx == 3) warningDict[WarningType.DesertTitanStart] = true;
                }
                else if (scan.Contains("finished"))
                {
                    warningDict[WarningType.TitanFinish] = true;
                }
                /*else if ((scanIdx == 0) && scan.Contains("detect"))
                {
                    warningDict[WarningType.Enemy] = true;
                }*/
                // else Lable_info.Dispatcher.Invoke(() => Lable_info.Content = "偵測中..");
                Trace.WriteLine("結束當次辨識，正在強制釋放記憶體");
                ocr.Dispose();
                bmp.Dispose();
                GC.Collect();
                isDetecting = false;
                Trace.WriteLine("釋放記憶體..完成, 結束該次偵測判斷");
            }
        }

        private Thread detect;
        void MainWaringTimerTick(object sender, EventArgs e)
        {
            Trace.WriteLine("-0-Detect Tick(5)..");
            if (!isDetecting)
            {
                using (Graphics g = Graphics.FromImage(gBmp))
                {
                    g.CopyFromScreen(0, 0, 0, 0, gBmp.Size);
                    // Lable_info.Dispatcher.Invoke(() => Lable_info.Content = "偵測中..");
                    detect = new Thread(DetectStatus);
                    detect.Start();
                }
            }
            Trace.WriteLine("-0-Detect Tick(5) Done");
        }

        Timer WaringTimer; // 警報
        private void InitMainWaringTimer()
        {
            Trace.WriteLine("正在啟動 Detect Tick(5)..");
            Lable_info.Content = "警報啟動完成";

            WaringTimer = new Timer { Interval = 5000 }; // 5s
            WaringTimer.Tick += new EventHandler(MainWaringTimerTick);
            WaringTimer.Start();
        }

        private string groupName;
        private void Wechat(string message)
        {
            Trace.WriteLine("處理警報資訊..");
            message = (message == "給你的警報起個名字 !") ? "副櫛龍偵測到敵人!" : message;
            wxBot.SendGroupMessage(groupName, message);
            Trace.WriteLine("警報資訊傳送完成");
        }

        private void SendMessageTick(object sender, EventArgs e)
        {
            Trace.WriteLine("-1-Send Tick(15)");
            if (warningDict[WarningType.Enemy])
            {
                Wechat(TB_Send.Dispatcher.Invoke(() => TB_Send.Text));
                Lable_info.Content = TB_Send.Text;
                warningDict[WarningType.Enemy] = false;
            }
            if (warningDict[WarningType.ForestTitanStart])
            {
                Wechat("警告：森林泰坦開始下載..");
                Lable_info.Content = "警告：森林泰坦開始下載..";
                warningDict[WarningType.ForestTitanStart] = false;
            }
            if (warningDict[WarningType.IceTitanStart])
            {
                Wechat("警告：冰霜泰坦開始下載..");
                Lable_info.Content = "警告：冰霜泰坦開始下載..";
                warningDict[WarningType.IceTitanStart] = false;
            }
            if (warningDict[WarningType.DesertTitanStart])
            {
                Wechat("警告：沙漠泰坦開始下載..");
                Lable_info.Content = "警告：沙漠泰坦開始下載..";
                warningDict[WarningType.DesertTitanStart] = false;
            }
            if (warningDict[WarningType.TitanFinish])
            {
                Wechat("警告：泰坦下載完成");
                Lable_info.Content = "警告：泰坦下載完成";
                warningDict[WarningType.TitanFinish] = false;
            }
            Trace.WriteLine("-1-Send Tick(15) Done");
        }

        Timer SendTimer; // 發送
        private void InitSendWaringTimer()
        {
            SendTimer = new Timer { Interval = 15000 }; // 15s
            SendTimer.Tick += new EventHandler(SendMessageTick);
            SendTimer.Start();
        }

        private void StartInit()
        {
            Trace.WriteLine("");
            Trace.WriteLine("開始初始化功能");
            //Lable_info.Content = "選擇了 " + CB_GroupList.Text;
            groupName = ComboB_GroupList.Text;
            Properties.Settings.Default.enemyMsg = TB_Send.Text;
            ComboB_GroupList.IsEnabled = false;
            InitBitmap();
            InitMainWaringTimer();
            InitSendWaringTimer();
            Trace.WriteLine("功能初始化完成");
            Trace.WriteLine("");
        }

        private void CheckB_ParaWarning_Checked(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("勾選開啟警報");
            Lable_info.Content = "警報啟動中..";
            StartInit();
        }

        private void CheckB_ParaWarning_Unchecked(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("取消警報");
            Lable_info.Content = "警報關閉";
            ComboB_GroupList.IsEnabled = true;
            WaringTimer.Stop();
            SendTimer.Stop();
        }

        private void B_LoginWX_Click(object sender, RoutedEventArgs e) => InitWX();

        private void ComboB_GroupList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) => CheckB_ParaWarning.IsEnabled = true;
    }
}
