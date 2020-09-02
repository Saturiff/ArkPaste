using ArkHarvester.Classes;
using System;
using System.Windows;
using System.Windows.Forms;
using TextBox = System.Windows.Controls.TextBox;

namespace ArkHarvester
{
    public partial class ClickArea : Window
    {
        private TextBox _x, _y;
        private HarvestObject _item;

        public ClickArea(HarvestObject item, TextBox x, TextBox y)
        {
            InitializeComponent();
            Left = Top = 0;
            _x = x;
            _y = y;
            _item = item;
            Width = SystemInformation.PrimaryMonitorSize.Width;
            Height = SystemInformation.PrimaryMonitorSize.Height;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _x.Text = Convert.ToString(Control.MousePosition.X);
            _y.Text = Convert.ToString(Control.MousePosition.Y);
            Hide();
            StorageDataFromTextBox();
            Close();
        }

        public void StorageDataFromTextBox()
        {
            _item.location = new System.Drawing.Point(int.Parse(_x.Text), int.Parse(_y.Text));

            Properties.Settings.Default.slotData.x = int.Parse(_x.Text);
            Properties.Settings.Default.slotData.y = int.Parse(_y.Text);
            Properties.Settings.Default.slotData.r = _item.GetColorAt(_item.location).R;
            Properties.Settings.Default.slotData.g = _item.GetColorAt(_item.location).G;
            Properties.Settings.Default.slotData.b = _item.GetColorAt(_item.location).B;
            Properties.Settings.Default.Save();

            _item.UpdateFromSlotData(Properties.Settings.Default.slotData);
        }
    }
}
