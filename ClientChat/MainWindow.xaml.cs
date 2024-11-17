using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ClientChat
{
    public partial class MainWindow : Window
    {
        private IPEndPoint serverEndPoint;
        private UdpClient client;
        private ObservableCollection<string> messages = new ObservableCollection<string>();
        private string userName;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = messages;
            client = new UdpClient();
            string address = ConfigurationManager.AppSettings["ServerAddress"];
            short port = short.Parse(ConfigurationManager.AppSettings["ServerPort"]);
            serverEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
        }

        private void Join_Button_Click(object sender, RoutedEventArgs e)
        {
            userName = Microsoft.VisualBasic.Interaction.InputBox("Enter your nickname:", "Join Chat");
            if (string.IsNullOrEmpty(userName))
            {
                MessageBox.Show("Nickname cannot be empty!");
                return;
            }

            string message = $"$<join>{userName}";
            SendMessage(message);

            joinButton.IsEnabled = false;
            leaveButton.IsEnabled = true;
            Listen(); // Почати слухати сервер після приєднання
        }

        private void Leave_Button_Click(object sender, RoutedEventArgs e)
        {
            string message = "$<leave>";
            SendMessage(message);

            joinButton.IsEnabled = true;
            leaveButton.IsEnabled = false;
        }

        private void Send_Button_Click(object sender, RoutedEventArgs e)
        {
            string message = msgTextBox.Text.Trim();
            if (string.IsNullOrEmpty(message))
            {
                MessageBox.Show("Message cannot be empty!");
                return;
            }

            SendMessage($"{userName}|{DateTime.Now:HH:mm:ss}|{message}");
            msgTextBox.Clear();
        }

        private async void SendMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(data, data.Length, serverEndPoint);
        }

        private async void Listen()
        {
            while (true)
            {
                var data = await client.ReceiveAsync();
                string message = Encoding.UTF8.GetString(data.Buffer);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    messages.Add(message);  // Додати отримане повідомлення до списку
                });
            }
        }
    }
}