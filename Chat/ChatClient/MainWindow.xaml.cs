using System.Windows;
using System.Windows.Input;
using ChatClient.ServiceChat;

namespace ChatClient
{
    public partial class MainWindow : Window, IServiceChatCallback
    {
        bool isConnected = false;
        ServiceChatClient client;
        int id;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectUser()
        {
            if (!isConnected)
            {
                client = new ServiceChatClient(new System.ServiceModel.InstanceContext(this));
                id = client.Connect(tbUserName.Text);
                tbUserName.IsEnabled = false;
                bConnDisconn.Content = "Disconect";
                isConnected = true;
            }
        }

        private void DisconnectUser()
        {
            if (isConnected)
            {
                client.Disconnect(id);
                client = null;
                tbUserName.IsEnabled = true;
                bConnDisconn.Content = "Connected";
                isConnected = false;
            }
        }

        private void Batton_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnected)
            {
                ConnectUser();
            }
            else
            {
                DisconnectUser();
            }
        }

        public void MessageCollback(string message)
        {
            lbChat.Items.Add(message);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DisconnectUser();
        }

        private void TbMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key==Key.Enter)
            {
                if (client != null)
                {
                    client.SendMssage(tbMessage.Text, id);
                    tbMessage.Text = string.Empty;
                }
            }
        }
    }
}
