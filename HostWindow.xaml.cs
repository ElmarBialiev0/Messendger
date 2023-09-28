using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Messanger
{
    public partial class HostWindow : Window
    {
        private TcpServer tcpServer = new();
        private TcpClient tcpClient = new();

        public static List<UserInfo> logs = new();

        public static List<string> users = new();
        public static List<string> logsprint = new();

        private bool isLogActive = false;

        public HostWindow()
        {
            InitializeComponent();
            logs.Clear();
            logsprint.Clear();

            tcpClient.UserName = ConnectWindow.login;
            IPEndPoint IP_point = new IPEndPoint(IPAddress.Any, 8888);
            tcpServer.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpServer.socket.Bind(IP_point);
            tcpServer.socket.Listen(1000);

            tcpClient.server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpClient.server.Connect("127.0.0.1", 8888);

            tcpServer.ListenClients(ListMesseges, UsersList);
            tcpClient.SendMessage($"&*&{tcpClient.UserName}");
        }



        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            DateTime time = DateTime.Now;
            tcpClient.SendMessage($"{tcpClient.UserName}: {MessegaBox.Text}");
            MessegaBox.Text = "";
        }

        private void SwitchUsersBtn_Click(object sender, RoutedEventArgs e)
        {
            isLogActive = !isLogActive;

            if (isLogActive)
            {
                UsersLog.Visibility = Visibility.Visible;
                UsersLog.ItemsSource = null;
                logsprint.Clear();
                foreach (var item in logs)
                {
                    logsprint.Add(item.ConnectionTime.ToString() + "\n" + item.Name);
                }
                UsersLog.ItemsSource = logsprint;

                UsersList.Visibility = Visibility.Hidden;
            }
            else
            {
                UsersLog.Visibility = Visibility.Hidden;
                UsersList.Visibility = Visibility.Visible;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Disconnect(ListMesseges, UsersList);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Disconnect(ListMesseges, UsersList);
        }

        private void Disconnect(ListBox ListMesseges, ListBox UsersList)
        {
            var clientsCopy = tcpServer.clients.ToList();
            tcpClient.SendMessage($"/close/");
            tcpServer.ListenClients(ListMesseges, UsersList);

            foreach (var client in clientsCopy)
            {
                tcpServer.DisconnectClient(client);
            }

            tcpServer.socket.Close();

            users.Clear();

            ConnectWindow connectWindow = new ConnectWindow();
            connectWindow.Show();
            this.Close();
        }
    }

    public class UserInfo
    {
        public DateTime ConnectionTime { get; set; }
        public string Name { get; set; }

        public UserInfo(DateTime connectTime, string name)
        {
            ConnectionTime = connectTime;
            Name = $"user: [{name}]";
        }
    }

    class TcpServer
    {
        public Socket socket;
        public List<Socket> clients = new List<Socket>();
        private TcpClient tcpClient = new();

        public async Task ListenClients(ListBox ListMesseges, ListBox UsersList)
        {
            while (true)
            {
                var client = await socket.AcceptAsync();
                clients.Add(client);

                GetMessage(client, ListMesseges, UsersList);

                
            }
        }

        public async Task GetMessage(Socket client, ListBox ListMesseges, ListBox UsersList)
        {
            string allClients = null;

            while (true)
            {
                byte[] bytes = new byte[1024];
                int bytesRecieved = await client.ReceiveAsync(bytes, SocketFlags.None);
                string message = Encoding.UTF8.GetString(bytes, 0, bytesRecieved);

                if (message.EndsWith("/disconnect") || message.EndsWith("/Disconnect"))
                {
                    clients.Remove(client);
                    HostWindow.users.Remove(message.Split(":").First());

                    ListMesseges.Items.Add($"({message.Split(":").First()}) disconnected.");

                    UsersList.ItemsSource = null;
                    UsersList.ItemsSource = HostWindow.users;
                    break;
                }
                else if (message.StartsWith("&*&"))
                {
                    HostWindow.users.Add(message.Substring(3));
                    UsersList.ItemsSource = null;
                    UsersList.ItemsSource = HostWindow.users;

                    foreach (var item in UsersList.Items)
                    {
                        if (item != null) allClients += " " + item;
                    }

                    foreach (var item in clients)
                    {
                        SendMessage(item, $"&*&{allClients}");
                    }
                    HostWindow.logs.Add(new UserInfo(DateTime.Now, HostWindow.users.Last()));
                }
                else 
                { 
                    ListMesseges.Items.Add($"[{DateTime.Now}] {message}");
                    foreach (var item in clients)
                    {
                        SendMessage(item, message);
                    }
                }
            }
        }

        private async Task SendMessage(Socket client, string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(bytes, SocketFlags.None);
        }

        public void DisconnectClient(Socket client)
        {
            if (clients.Contains(client))
            {
                foreach (var item in clients)
                {
                    SendMessage(item, $"/close/");
                }
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                clients.Remove(client);
                HostWindow.users.Remove(tcpClient.UserName);
            }
        }

    }
}
