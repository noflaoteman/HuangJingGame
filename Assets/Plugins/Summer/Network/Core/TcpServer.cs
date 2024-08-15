using Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Summer.Network
{
    //服务器端的Socket封装，这个类只在服务器中使用，是基于Connection的封装
    /// <summary>
    /// 负责监听TCP网络端口，异步接收Socket连接
    /// -- Connected        有新的连接
    /// -- DataReceived     有新的消息
    /// -- Disconnected     有连接断开
    /// Start()         启动服务器
    /// Stop()          关闭服务器
    /// IsRunning     是否正在运行
    /// </summary>
    public class TcpServer
    {
        private IPEndPoint endPoint;
        private Socket serverSocket;    //服务端监听对象
        private int backlog = 100;      //可以排队接受的传入连接数

        public bool IsRunning
        {
            get { return serverSocket != null; }
        }


        public delegate void ConnectedCallback(Connection conn);
        public delegate void DataReceivedCallback(Connection conn, Google.Protobuf.IMessage data);
        public delegate void DisconnectedCallback(Connection conn);


        //客户端接入事件
        public event EventHandler<Socket> SocketConnected;
        //事件委托：新的连接
        public event ConnectedCallback Connected;
        //事件委托：收到消息，当Connection收到就转发出去
        public event DataReceivedCallback DataReceived;
        //事件委托：连接断开
        public event DisconnectedCallback Disconnected;

        public TcpServer(string host, int port)
        {
            endPoint = new IPEndPoint(IPAddress.Parse(host), port);
        }
        public TcpServer(string host, int port, int backlog) : this(host, port)
        {
            this.backlog = backlog;
        }

        /// <summary>
        /// 初始化服务端的Socket相关
        /// </summary>
        public void Start()
        {
            if (!IsRunning)
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(endPoint);
                serverSocket.Listen(backlog);
                Log.Information("开始监听端口：" + endPoint.Port);
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += OnAccept; //当有人连入的时候
                serverSocket.AcceptAsync(args);
            }
            else
            {
                Log.Information("TcpServer is already running.");
            }
        }

        /// <summary>
        /// 有客户端连入的时候执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAccept(object? sender, SocketAsyncEventArgs e)
        {
            //连入的客户端
            Socket clientSocket = e.AcceptSocket;
            //继续接收下一位
            e.AcceptSocket = null;
            serverSocket.AcceptAsync(e);
            //真的有人连进来
            if (e.SocketError == SocketError.Success)
            {
                if (clientSocket != null)
                {
                    OnSocketConnected(clientSocket);
                }
            }

        }

        /// <summary>
        /// 处理客户端的Socket
        /// </summary>
        /// <param name="clientSocket"></param>
        private void OnSocketConnected(Socket clientSocket)
        {
            SocketConnected?.Invoke(this, clientSocket);
            Connection clientConn = new Connection(clientSocket);
            //从Connetion中获取数据传递给外部的意思
            clientConn.OnDataReceived += (conn, data) => DataReceived?.Invoke(conn, data);
            //从Connetion中获取了Conn数据传递给外部的意思
            clientConn.OnDisconnected += (conn) => Disconnected?.Invoke(conn);
            //有客户端连接那么就触发连接的委托
            Connected?.Invoke(clientConn);
        }

        public void Stop()
        {
            if (serverSocket == null)
                return;
            serverSocket.Close();
            serverSocket = null;
        }

    }

}



