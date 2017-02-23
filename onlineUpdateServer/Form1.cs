using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace onlineUpdateServer
{
    public partial class Form1 : Form
    {
        //服务器套接字
        Socket serverSocket;
        //客户端
        List<user> clientSockets = new List<user>();
        //文件信息
        string fileName = "";
        class FileInformation
        {
            public byte[] fileName { get; set; }
            public byte[] fileData { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.listBox1.Items.Clear();
            //获取文件信息
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath + @"\..\");
            fileName = fileInfo.FullName + "UpdateFile";
            DirectoryInfo files= new DirectoryInfo(fileName);

            //遍历指定文件夹下面的文件显示出来
            foreach(FileInfo nextfile in files.GetFiles())
            {
                this.listBox1.Items.Add(nextfile);
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

            serverSocket.Listen(1000);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), serverSocket);
        }
        /*接收到客户端连接*/
        private void AcceptCallback(IAsyncResult ar)
        {
            Socket serverSocket = (Socket)ar.AsyncState;
            Socket clientSocket = serverSocket.EndAccept(ar);

            user client = new user();
            client.workSocket = clientSocket;
            //监听客户端加上去
            clientSockets.Add(client);
            this.listBox2.Items.Add("1");

            clientSocket.BeginReceive(client.buffer,0,user.bufferSize, 0,
                new AsyncCallback(ReceiveCallback),client);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), serverSocket);
        }
        /*接收消息回调函数*/
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
        /*向指定的客户端发送心跳包回应*/
        private void Send(user client)
        {
            byte[] buffer = Encoding.Default.GetBytes("successful");
            client.workSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback),
                     client);
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

        private void button2_Click(object sender, EventArgs e)
        {
            //开启发送文件线程
            Thread tdSendFile = new Thread(new ThreadStart(SendFile));
            tdSendFile.IsBackground = true;
            tdSendFile.Start();
        }

        /*发送文件*/
        public void SendFile()
        {
            FileInformation fileSend = new FileInformation();
            //找到指定文件夹下面的第一个文件，为他创建流
            fileName =Application.ExecutablePath+@"\..\";
            //FileInfo fileInfo = new FileInfo(fileName + "UpdateFile\\"+this.listBox1.Items[0].ToString());

            FileInfo fileInfo = new FileInfo(this.textBox1.Text);
            FileStream fileStream = fileInfo.OpenRead();

            //得到文件名
            byte[] filename = Encoding.Default.GetBytes(fileInfo.Name);
            fileSend.fileName = Encoding.Default.GetBytes(fileInfo.Name);

            //得到文件数据
            byte[] data = new byte[fileInfo.Length];
            fileStream.Read(data, 0, data.Length);
            fileSend.fileData = data;

            //序列化数据
            JavaScriptSerializer jsser = new JavaScriptSerializer();
            jsser.MaxJsonLength = Int32.MaxValue;
            string json = jsser.Serialize(fileSend);

            byte[] SendData = Encoding.Default.GetBytes(json);
            byte[] SendDataFirst = Encoding.Default.GetBytes("1234567");
            byte[] SendDataEnd = Encoding.Default.GetBytes("7654321");
            try
            {
                FileInformation file = jsser.Deserialize<FileInformation>(json);
                MessageBox.Show(Encoding.Default.GetString(file.fileName));
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            try
            {       
                for (int j = 0; j < clientSockets.Count; j++)
                {
                    clientSockets[j].workSocket.BeginSend(SendDataFirst, 0, SendDataFirst.Length, SocketFlags.None,
                        new AsyncCallback(SendCallback), clientSockets[j]);
                    clientSockets[j].workSocket.BeginSend(SendData, 0, SendData.Length, SocketFlags.None,
                        new AsyncCallback(SendCallback), clientSockets[j]);
                    clientSockets[j].workSocket.BeginSend(SendDataEnd, 0, SendDataEnd.Length, SocketFlags.None,
                        new AsyncCallback(SendCallback), clientSockets[j]);
                }
                MessageBox.Show("上传成功!");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(this.openFileDialog1.FileName != "")
            {
                if(this.openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    this.textBox1.Text = this.openFileDialog1.FileName;
                }
            }
        }
    }
}
