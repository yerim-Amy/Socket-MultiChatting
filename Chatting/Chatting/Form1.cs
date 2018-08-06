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

namespace Chatting
{
    public partial class Form1 : Form
    {
        IPEndPoint ep_my;
        public const int MessageSize = 1024;
        byte[] byteMessage;

        private static Socket serverSocket;
        private static byte[] _buffer = new byte[1024];
        private static List<Socket> _clientSockets = new List<Socket>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //폼 로드되면 내 IP주소 가져오기
                tb_myIP.Text = getMyIPAddress();
                tb_myName.Text = getMyIPAddress();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Load Error: " + ex.Message);
            }

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

        private void btn_connect_Click(object sender, EventArgs e)
        {
            try
            {
                 ep_my = new IPEndPoint(IPAddress.Parse(tb_myIP.Text), Convert.ToInt32(tb_myPort.Text));
                serverSocket.Bind(ep_my);
                serverSocket.Listen(5);
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

                btn_connect.Text = "시작";
                btn_connect.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bind Error: " + ex.Message);
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket socket = serverSocket.EndAccept(ar);
            _clientSockets.Add(socket);
            lb_chat.Items.Add("Client(" + socket.RemoteEndPoint + ") 와 연결되었습니다");
            
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            int received = socket.EndReceive(ar);
            byte[] dataBuf = new byte[received];
            Array.Copy(_buffer, dataBuf, received);

            string text = Encoding.UTF8.GetString(dataBuf);
            string searchText = ",";
            string clientName = text.Substring(0, text.IndexOf(searchText));
            string clientMessage = text.Substring(text.IndexOf(searchText) + 1);

            lb_chat.Items.Add("Client(" + clientName + ") : " + clientMessage);
       
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }
       
        private void btn_send_Click(object sender, EventArgs e)
        {
            if (tb_send.Text.Length <= 0)
            {
                MessageBox.Show("텍스트를 입력하세요.");
            }

            byte[] buffer = Encoding.UTF8.GetBytes(tb_send.Text);

            for (int i = _clientSockets.Count - 1; i >= 0; i--) //Connect된 Client에게 데이터 전송
            {
                Socket socket = _clientSockets[i];
                try 
                { 
                    socket.Send(buffer); 
                }
                catch (Exception)
                {
                    MessageBox.Show("Clients Send Error");
                    socket.Dispose();//해제
                    _clientSockets.RemoveAt(i);//리스트에서 삭제
                }
            }
            lb_chat.Items.Add("Server : " + tb_send.Text);
            tb_send.Text = "";
        }

        private void tb_myIP_TextChanged(object sender, EventArgs e)
        {

        }

       
    }
}
