using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Messanger
{
    public partial class ConnectWindow : Window
    {
        public static string ip;
        public static string login;
        public ConnectWindow()
        {
            InitializeComponent();
        }

        private void CreateServerBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LoginBox.Text.Length > 0)
                {
                    login = LoginBox.Text;
                    HostWindow hostWindow = new HostWindow();
                    hostWindow.Show();
                    this.Close();
                }
                else MessageBox.Show("Введите никнейм");
            }
            catch 
            {
                MessageBox.Show("сервер уже запущен");
            }
        }

        private void ConnectServerBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LoginBox.Text.Length > 0)
                {
                    login = LoginBox.Text;
                    ip = IPBox.Text;
                    ChatWindow chatWindow = new ChatWindow();
                    chatWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Введите никнейм");
                }
            }
            catch 
            {
                MessageBox.Show("сервер не запущен");
            }
        }
    }
}
