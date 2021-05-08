using MainWpfApp;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

public class TcpClient
    {
        private Socket socketsend;
        private Thread tcpClientThread;
        NetworkStream clientStream;
        public MainWindow mainwin = (MainWindow)Application.Current.MainWindow;

    //public String ipStr = "192.168.1.160";  //服务器ip

    public String ipStr = "127.0.0.1";  //tcpserver ip 调式用 
        //public String ipStr = "192.168.31.235";  //服务器ip
        public String portStr = "5000";  //端口号

        public int TcpConnFlag = 0; //tcp连接标志 0表示未连接 1表示连接

        public void TcpConnect()
        {
                try {
                    //while (TcpConnFlag != 1)
                    //{
                        socketsend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        // socketsend.Blocking = false;

                        //连接对应的端口
                        IPAddress ip = IPAddress.Parse(ipStr);
                        IPEndPoint port = new IPEndPoint(ip, Convert.ToInt32(portStr));
                        socketsend.Connect(port);
                        if (socketsend.Connected == true)
                        {
                            Console.WriteLine(socketsend.RemoteEndPoint + ": 连接成功!");
                            TcpConnFlag = 1;
                            return;
                        }
                    //}
                
                }
                catch
                {
                    Console.WriteLine(socketsend.RemoteEndPoint + ": 连接失败!");
                }

        }

    public void TcpConnectThreadStart() {
        Thread tcpCon = new Thread(TcpConnect);
        tcpCon.Start();
    }

        //开辟TCP线程
        public void TcpClientThreadStart()
        {
            tcpClientThread = new Thread(TcpThread);
            //tcpClientThread.IsBackground = true; //后台程序
            tcpClientThread.Start();
        }
        /************************以下为TCP线程**************************/
        //TCP线程
        public void TcpThread()
        {
            byte[] dataBuffer = new byte[1024 * 32];
            int dataBufferLen;
            byte[] getDataByt = { (byte)0xff, (byte)0x03 };
            while (true)
            {
            if (mainwin.IsLockWave == false)
            {

                try
                {
                    TcpSendData(getDataByt);
                    dataBufferLen = 0;
                    dataBufferLen = socketsend.Receive(dataBuffer); //阻塞连接
                    //接收到的数据长度为0时表示连接断开，跳出循环
                    if (dataBufferLen == 0)
                    {
                        TcpConnFlag = 0;
                        Console.WriteLine("连接断开，正在重新连接!");
                        TcpConnect();
                    }
                    RecDataHandle(dataBuffer, dataBufferLen);

                }
                catch (Exception e)
                {
                    Console.WriteLine("TcpThread erro：" + e.Message);
                    Console.WriteLine("正在重新连接!");
                    TcpConnFlag = 0;
                    TcpConnect();

                }
                Thread.Sleep(300);
            }
            else {
                Thread.Sleep(1000);
            }
                
            }
        }

        //接收数据处理函数
        public virtual void RecDataHandle(byte[] mesBuffer, int bytesReadLen)
        {
            Console.WriteLine("接收数据处理函数 TCPServer类");
        }

        //发送数据
        public bool TcpSendData(byte[] sendByt)
        {
            try
            {
                socketsend.Send(sendByt);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


    }





