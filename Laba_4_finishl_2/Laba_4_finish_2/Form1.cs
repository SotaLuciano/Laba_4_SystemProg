using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;


    public partial class Form1 : Form
    {
        string strIp = "127.0.0.1";
        TcpClient Client = null;
        IPAddress ip;
        IPEndPoint ep;

        public Form1()
        {
            InitializeComponent();
            ip = IPAddress.Parse(strIp);
            ep = new IPEndPoint(ip, 2025);
        }

        public void TheardReadClient(object tm)
        {
            TcpClient Client = tm as TcpClient;
            while (true)
            {
                try
                {
                    byte[] bytes = new byte[10086];
                    BinaryFormatter formatter = new BinaryFormatter();
                    NetworkStream ns = Client.GetStream();
                    ns.Read(bytes, 0, bytes.Length);
                    MemoryStream memStream = new MemoryStream(bytes);
                    memStream.Close();

                    String str = Encoding.UTF8.GetString(bytes);
                    string[] words = str.Split('\0');
                    str = "";
                    for (int i = 0; i < words.Length; i++)
                    {
                        if (words[i] != "")
                            str += words[i];
                    }
                    if (str != "")
                    {
                        this.Invoke((MethodInvoker)(() => listBox1.Items.Add(str + "\n")));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            Client.Close();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
            {
                textBox2.Enabled = false;
                button3.Enabled = false;
                listBox1.Enabled = true;
                button1.Enabled = true;
                textBox1.Enabled = true;

                Client = new TcpClient();
                Client.Connect(ep);
                try
                {
                    ParameterizedThreadStart ts = new ParameterizedThreadStart(TheardReadClient);
                    Thread th = new Thread(ts);
                    th.IsBackground = true;
                    th.Start(Client);

                    TcpClient NewClient = Client;
                    MemoryStream memStream = new MemoryStream();
                    BinaryFormatter formatter = new BinaryFormatter();

                    byte[] secondString = Encoding.Unicode.GetBytes(textBox2.Text + "\0");
                    NetworkStream ns = NewClient.GetStream();
                    ns.Write(secondString, 0, secondString.Length);
                    memStream.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                MessageBox.Show("Input name!");
        }
        private void button1_Click(object sender, EventArgs e)
        {
            TcpClient NewClient = Client;
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            byte[] secondString = Encoding.Unicode.GetBytes(textBox1.Text);
            NetworkStream ns = NewClient.GetStream();
            ns.Write(secondString, 0, secondString.Length);
            memStream.Close();
            textBox1.Text = "";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                TcpClient NewClient = Client;
                MemoryStream memStream = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                byte[] secondString = Encoding.Unicode.GetBytes("DisconnectedSYSCODE");
                NetworkStream ns = NewClient.GetStream();
                ns.Write(secondString, 0, secondString.Length);
                Client.Close();
            }
            catch (Exception ex)
            { }
        }
    }

