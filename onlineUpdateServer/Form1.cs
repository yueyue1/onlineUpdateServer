using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace onlineUpdateServer
{
    public partial class Form1 : Form
    {
        //服务器套接字
        Socket serverSocket;
        //客户端
        List<user> clientSockets = new List<user>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog1.FileName != "")
            {
                if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    this.textBox1.Text = this.openFileDialog1.FileName;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            EndPoint localEP = new IPEndPoint(IPAddress.Any, 5555);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                serverSocket.Bind(localEP);
            }
            catch
            {
                Console.WriteLine("绑定出现异常");
            }
            serverSocket.Listen(100);

            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), serverSocket);
        }
        private void AcceptCallback(IAsyncResult ar)
        {
            Socket serverSocket = (Socket)ar.AsyncState;
            Socket clientSocket = serverSocket.EndAccept(ar);

            user client = new user();
            client.workSocket = clientSocket;
            //监听客户端加上去
            clientSockets.Add(client);

            clientSocket.BeginReceive(client.buffer,0,user.bufferSize, 0,
                new AsyncCallback(ReceiveCallback),client);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), serverSocket);
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            user client = (user)ar.AsyncState;
            try
            {
                int receivelength = client.workSocket.EndReceive(ar);

                string message = Encoding.Default.GetString(client.buffer, 0, receivelength);
                if (message == "1111111111")
                {
                    Send(client);
                }
                client.workSocket.BeginReceive(client.buffer, 0, user.bufferSize, 0,
                    new AsyncCallback(ReceiveCallback), client);
            }
            catch
            {
                //MessageBox.Show("客户端已关闭");
            }
        }
        private void Send(user client)
        {
            byte[] buffer = Encoding.Default.GetBytes("successful");
            client.workSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback),
                     client);
            //MessageBox.Show("服务器发送1111成功");
        }
        private void SendCallback(IAsyncResult ar)
        {
            user SocketEnd = (user)ar.AsyncState;
            try
            {
                int endlength = SocketEnd.workSocket.EndSend(ar);
            }
            catch
            { }
        }
    }
}
