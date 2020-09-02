using ArkHarvester.Classes;
using System;
using System.Windows;
using System.Windows.Forms;
using TextBox = System.Windows.Controls.TextBox;

namespace ArkHarvester
{
    public partial class ClickArea : Window
    {
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

        private TextBox _x, _y;
        private HarvestObject _item;
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
            _item.location.X = int.Parse(_x.Text);
            _item.location.Y = int.Parse(_y.Text);

            Properties.Settings.Default.M_Box_item_position_x = int.Parse(_x.Text);
            Properties.Settings.Default.M_Box_item_position_y = int.Parse(_y.Text);
            Properties.Settings.Default.M_Color_itemR = _item.GetColorAt(_item.location).R;
            Properties.Settings.Default.M_Color_itemG = _item.GetColorAt(_item.location).G;
            Properties.Settings.Default.M_Color_itemB = _item.GetColorAt(_item.location).B;
            Properties.Settings.Default.Save();

            _item.itemR = Properties.Settings.Default.M_Color_itemR;
            _item.itemG = Properties.Settings.Default.M_Color_itemG;
            _item.itemB = Properties.Settings.Default.M_Color_itemB;

            //change _item location to dx or dy
            _item.screen2dx = _item.location.X * 65535 / SystemInformation.PrimaryMonitorSize.Width;
            _item.screen2dy = _item.location.Y * 65535 / SystemInformation.PrimaryMonitorSize.Height;
        }
    }
}
