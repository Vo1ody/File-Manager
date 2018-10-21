using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace File_Manager_WPF
{
    /// <summary>
    /// Interaction logic for FtpClientWindow.xaml
    /// </summary>
    public partial class FtpClientWindow : Window
    {
        public Client client;
        private FtpDelegate ftpd;
        public FtpClientWindow(FtpDelegate sender)
        {
            InitializeComponent();
            ftpd = sender;
        }
        private void btn_connect_Click(object sender, RoutedEventArgs e)
        {
            client = new Client(Host.Text, Login.Text, Password.Password);
            ftpd(client);
            Close();
        }
        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            Host.Text = Login.Text = "";
            Password.Clear();
        }
        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
