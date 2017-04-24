using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

    public struct NameAndSoket
    {
        public string Name;
        public TcpClient Client;
    };
    class Program
    {

      //  string strIp = "127.0.0.1";
        private static Semaphore _pool = new Semaphore(2, 2);
        public List<NameAndSoket> NamesAndSokets = new List<NameAndSoket>();

        public void TheardRead(object ts)
        {
            NameAndSoket tm = (NameAndSoket)ts;
            TcpClient Client = tm.Client;
            while (true)
            {
            //READ
                byte[] bytes = new byte[10086];
                BinaryFormatter formatter = new BinaryFormatter();
                NetworkStream ns = Client.GetStream();
                ns.Read(bytes, 0, bytes.Length);

                String str = Encoding.UTF8.GetString(bytes);
                string[] words = str.Split('\0');
                str = "";
                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i] != "")
                        str += words[i];
                }
                Console.WriteLine("Server. Client. " + tm.Name + ": " + str);
                // IS DISCONECTED
                bool isDisconnectedSYSCODE = false;
                if (str == "DisconnectedSYSCODE")
                {
                    for (int i = 0; i < NamesAndSokets.Count; i++)
                    {
                        if (NamesAndSokets[i].Name == tm.Name)
                        {
                            NamesAndSokets.Remove(tm);
                            isDisconnectedSYSCODE = true;
                        }
                    }
                }
                if (isDisconnectedSYSCODE)
                    break;
 
               // OTHER CLIENT
                if (str != "")
                {
                    for (int i = 0; i < NamesAndSokets.Count; i++)
                    {
                        TcpClient NewClient = NamesAndSokets[i].Client;
                        MemoryStream memStreamWrite = new MemoryStream();
                        BinaryFormatter formatterWrite = new BinaryFormatter();
                        byte[] secondString = Encoding.Unicode.GetBytes(tm.Name + ": " + str);

                        //Console.WriteLine("Send to " + NamesAndSokets[i].Name + ": '" + tm.Name + ": " + str + "'");
                        NetworkStream nsWrite = NewClient.GetStream();
                        nsWrite.Write(secondString, 0, secondString.Length);
                        memStreamWrite.Close();
                    }
                }
            }
            Client.Close();
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Start server: ");
            string strIp = "127.0.0.1";
            IPAddress ip = IPAddress.Parse(strIp);
            IPEndPoint ep = new IPEndPoint(ip, 2025);
            TcpListener ServerSocket = new TcpListener(ep);
            Program p = new Program();
            ServerSocket.Start(10);
            try
            {
                while (true)
                {
                    TcpClient Client = ServerSocket.AcceptTcpClient();
                    if (Client.Connected)
                    {
                        if (_pool.WaitOne())
                        {
                            try
                            {
                                byte[] bytes = new byte[1024];
                                BinaryFormatter formatter = new BinaryFormatter();
                                NetworkStream ns = Client.GetStream();
                                ns.Read(bytes, 0, bytes.Length);

                                String str = Encoding.UTF8.GetString(bytes);
                                string[] words = str.Split('\0');
                                str = "";
                                for (int i = 0; i < words.Length; i++)
                                {
                                    if (words[i] != "")
                                        str += words[i];
                                }

                                NameAndSoket tmp;
                                tmp.Name = str;
                                tmp.Client = Client;

                                Console.WriteLine("Server. Client. Connected: " + str);

                                p.NamesAndSokets.Add(tmp);

                                ParameterizedThreadStart ts = new ParameterizedThreadStart(p.TheardRead);
                                Thread th = new Thread(ts);
                                th.IsBackground = true;
                                th.Start(tmp);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            finally
                            {
                                _pool.Release();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
