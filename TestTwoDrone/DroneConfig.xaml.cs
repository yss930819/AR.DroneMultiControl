using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TestTwoDrone
{
    /// <summary>
    /// DroneConfig.xaml 的交互逻辑
    /// </summary>
    public partial class DroneConfig : Window
    {
        public TestViewItem d;

        public Action OnSaved;

        public DroneConfig()
        {
            InitializeComponent();

        }

        /// <summary>
        /// 初始化控件内容
        /// 读取Drone中的配置信息
        /// </summary>
        /// <returns></returns>
        private void InitControl()
        {
            lb_name.Content = d.Name;
            lb_Info.Content = "ID:" + d.ID + "    IP:" + d.IP;

            tb_aimx.Text = d._aimX + "";
            tb_aimy.Text = d._aimY + "";
            tb_aimz.Text = d._aimZ + "";

            tb_pitch_P.Text = d.P_pitch + "";
            tb_pitch_D.Text = d.D_pitch + "";

            tb_roll_P.Text = d.P_roll + "";
            tb_roll_D.Text = d.D_roll + "";

            tb_gaz_P.Text = d.P_gaz + "";


        }

        private void Window_SourceInitialized_1(object sender, EventArgs e)
        {
            InitControl();
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            d._aimX = float.Parse(tb_aimx.Text);
            d._aimY = float.Parse(tb_aimy.Text);
            d._aimZ = float.Parse(tb_aimz.Text);

            d.P_pitch = float.Parse(tb_pitch_P.Text);
            d.D_pitch = float.Parse(tb_pitch_D.Text);

            d.P_roll = float.Parse(tb_roll_P.Text);
            d.D_roll = float.Parse(tb_roll_D.Text);

            d.P_gaz = float.Parse(tb_gaz_P.Text);

            if (OnSaved != null)
            {
                OnSaved();
            }

            this.Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}
