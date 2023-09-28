using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Messanger
{
    public partial class ChatWindow : Window
    {
        private TcpClient tcpClient = new TcpClient();

        public ChatWindow()
        {
            InitializeComponent();

            tcpClient.UserName = ConnectWindow.login;

            tcpClient.server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpClient.server.Connect(ConnectWindow.ip, 8888);

            tcpClient.GetMessage(ListMesseges, UsersList, this);
            tcpClient.SendMessage($"&*&{tcpClient.UserName}");

            UsersList.ItemsSource = null;
            UsersList.ItemsSource = HostWindow.users;
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessegaBox.Text.Equals("/disconnect") || MessegaBox.Text.Equals("/Disconnect"))
            {
                Disconnect();
            }
            else
            {
                DateTime time = DateTime.Now;
                tcpClient.SendMessage($"{tcpClient.UserName}: {MessegaBox.Text}");

                MessegaBox.Text = "";
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
        }

        private void Disconnect() 
        {
            tcpClient.SendMessage("/disconnect");
            tcpClient.DisconnectAsync().Wait();
            ConnectWindow connectWindow = new ConnectWindow();
            connectWindow.Show();
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Disconnect();
        }
    }

    public class TcpClient
    {
        public Socket server;
        public string UserName;

        public async Task GetMessage(ListBox ListMesseges, ListBox UsersList, Window wnd)
        {
            while (true)
            {
                byte[] bytes = new byte[1024];
                await server.ReceiveAsync(bytes, SocketFlags.None);
                string message = Encoding.UTF8.GetString(bytes);

                if (message.StartsWith("&*&"))
                {
                    List<string> users = message.Substring(4).Split(' ').ToList();
                    HostWindow.users = users;
                    UsersList.ItemsSource = null;
                    UsersList.ItemsSource = users;
                }
                else if (message.EndsWith("/Disconnect") || message.EndsWith("/disconnect"))
                {
                    HostWindow.users.Remove(message.Split(":").First());

                    ListMesseges.Items.Add($"({message.Split(":").First()}) disconnected.");

                    UsersList.ItemsSource = null;
                    UsersList.ItemsSource = HostWindow.users;
                }
                else if (message.Contains("/close/"))
                {
                    ConnectWindow connectWindow = new ConnectWindow();
                    connectWindow.Show();
                    wnd.Close();
                    DisconnectAsync();
                    
                }
                else ListMesseges.Items.Add($"[{DateTime.Now}] {message}");

                UsersList.ItemsSource = null;
                UsersList.ItemsSource = HostWindow.users;
            }
        }

        public async Task SendMessage(string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await server.SendAsync(bytes, SocketFlags.None);
        }

        public async Task DisconnectAsync()
        {
            server.Close();
        }
    }
}
