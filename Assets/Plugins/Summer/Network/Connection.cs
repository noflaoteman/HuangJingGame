using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Serilog;
using Google.Protobuf.Reflection;
using Summer.Core;

namespace Summer.Network
{
    /// <summary>
    /// 通用网络连接，可以继续封装此类实现功能拓展，在SocketReceiver基础上封装了发送
    /// 职责：发送消息，关闭连接，断开回调，接收消息回调，
    /// </summary>
    public class Connection
    {

        public delegate void DataReceivedCallback(Connection conn, IMessage data);
        public delegate void DisconnectedCallback(Connection conn);

        private Socket _socket;

        public Socket Socket
        {
            get { return _socket; }
        }


        /// <summary>
        /// 接收到数据
        /// </summary>
        public DataReceivedCallback OnDataReceived;
        /// <summary>
        /// 连接断开，声明的委托用于告诉外部
        /// </summary>
        public DisconnectedCallback OnDisconnected;

        public Connection(Socket socket)
        {
            this._socket = socket;
            //初始化接收器
            SocketReceiver socketRcve = new SocketReceiver(socket);
            socketRcve.DataReceived += _received;
            //当SocketReceiver断开连接会触发Connection断开的委托
            socketRcve.Disconnected += () => OnDisconnected?.Invoke(this);
            socketRcve.Start();
        }

        //SocketReceiver收到数据的时候执行放入队列里面
        private void _received(byte[] data)
        {
            //解析消息体的编码是什么
            ushort typeCode = GetUShort(data, 0);
            //得到真正的消息内容
            IMessage msg = ProtoHelper.ParseFrom(typeCode, data, 2, data.Length - 2);

            if (MessageRouter.Instance.Running)
            {
                //把收到的消息内容加到分发器的队列里面
                MessageRouter.Instance.AddMessage(this, msg);
            }

            OnDataReceived?.Invoke(this, msg);
        }


        /// <summary>
        /// 主动关闭连接
        /// </summary>
        public void Close()
        {
            try { _socket.Shutdown(SocketShutdown.Both); } catch { }
            _socket.Close();
            _socket = null;
            OnDisconnected?.Invoke(this);
        }


        #region 发送网络数据包
        public void Send(IMessage message)
        {
            using (DataStream ds = DataStream.Allocate())
            {
                //消息的构成：
                //消息总长度+消息类型+消息体
                //4字节+2字节+X字节

                int code = ProtoHelper.SeqCode(message.GetType());
                //写入消息的总长度
                ds.WriteInt(message.CalculateSize() + 2);
                //写入消息的类型
                ds.WriteUShort((ushort)code);
                //写入probuf消息内容
                message.WriteTo(ds);

                //发送这条消息
                this.SocketSend(ds.ToArray());
            }
        }

        //通过socket发送原生数据
        private void SocketSend(byte[] data)
        {
            this.SocketSend(data, 0, data.Length);
        }
        //通过socket发送原生数据
        private void SocketSend(byte[] data, int offset, int len)
        {
            lock (this)
            {
                if (_socket.Connected)
                {
                    _socket.BeginSend(data, offset, len,
                                        SocketFlags.None,
                                        new AsyncCallback(SendCallback), _socket);
                }
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            // 发送的字节数
            int len = _socket.EndSend(ar);
        }
        #endregion


        //获取大端字节的ushort值
        private ushort GetUShort(byte[] data, int offset)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (ushort)((data[offset] << 8) | data[offset + 1]);
            }
            else
            {
                return (ushort)((data[offset + 1] << 8) | data[offset]);
            }
        }


    }
}
