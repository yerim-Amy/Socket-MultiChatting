using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace ClientChatting
{
    public partial class Form1 : Form
    {
        private static Socket clientSocket;
        IPEndPoint ep_other;
        private static byte[] _buffer;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _buffer = new byte[1024];
            tb_otherIP.Text = getMyIPAddress();
            tb_myName.Text = getMyIPAddress();
        }
        public string getMyIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            throw new Exception("Ipv4주소 없습니다.");
        }

        private void btn_send_Click(object sender, EventArgs e)
        {
            if(tb_send.Text.Length<=0)
            {
                MessageBox.Show("텍스트를 입력하세요.");
            }

            byte[] buffer = Encoding.UTF8.GetBytes(tb_myName.Text + "," + tb_send.Text);
            clientSocket.Send(buffer);
            lb_chat.Items.Add("Client("+tb_myName.Text+") : " + tb_send.Text);
            tb_send.Text = "";
        }

        private void btn_connect_Click(object sender, EventArgs e)
        {
            if (clientSocket.Connected)
            {
                MessageBox.Show("이미 연결되어 있습니다");
                return;
            }

            try
            {
                ep_other = new IPEndPoint(IPAddress.Parse(tb_otherIP.Text), Convert.ToInt32(tb_otherPort.Text));
                clientSocket.Connect(ep_other);
                clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), clientSocket);
            }
            catch (Exception)
            {
                MessageBox.Show("Client Connect Error");
            }
            lb_chat.Items.Add("Server와 연결되었습니다");
            btn_connect.Text = "연결됨";
            btn_connect.Enabled = false;
        }
               
        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            int received = socket.EndReceive(ar);
            byte[] dataBuf = new byte[received];
            Array.Copy(_buffer, dataBuf, received);

            string text = Encoding.UTF8.GetString(dataBuf);

            lb_chat.Items.Add("Server : " + text);
                                
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);

        }      
    }
}
