using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Summer.Network
{
    /// <summary>
    /// 最底层Socket的封装
    /// </summary>
    public class SocketReceiver
    {

        public delegate void DataReceivedEventHandler(byte[] data);
        public delegate void DisconnectedEventHandler();

        //成功收到消息的委托事件，委托传的是一个消息内容data
        public event DataReceivedEventHandler DataReceived;
        //连接断开的委托事件
        public event DisconnectedEventHandler Disconnected;

        //缓冲区
        private byte[] buffer;
        //每次读取的开始位置
        private int startIndex = 0;

        private Socket _Socket;

        //构造函数，给缓冲区和Socket赋值
        public SocketReceiver(Socket socket) : this(socket, 1024 * 64)
        { }
        public SocketReceiver(Socket socket, int bufferSize)
        {
            this._Socket = socket;
            buffer = new byte[bufferSize];
        }

        public void Start()
        {
            BeginReceive();
        }

        private void BeginReceive()
        {
            _Socket.BeginReceive(buffer, startIndex, buffer.Length - startIndex,
                                 SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int len = _Socket.EndReceive(ar);
                // 0代表连接失败
                if (len == 0)
                {
                    _disconnected();
                    return;
                }
                //解析数据
                doReceive(len);
                //继续接收
                BeginReceive();
            }
            catch (SocketException)
            {
                _disconnected();
            }


        }

        private void doReceive(int len)
        {
            //解析数据
            int remain = startIndex + len;
            int offset = 0;
            while (remain > 4)
            {
                int msgLen = GetInt32BE(buffer, offset);
                if (remain < msgLen + 4)
                {
                    break;
                }
                //解析消息
                byte[] data = new byte[msgLen];
                Array.Copy(buffer, offset + 4, data, 0, msgLen);
                try { DataReceived?.Invoke(data); } catch { Log.Debug("消息解析异常"); }

                offset += msgLen + 4;
                remain -= msgLen + 4;
            }

            if (remain > 0)
            {
                Array.Copy(buffer, offset, buffer, 0, remain);
            }

            startIndex = remain;

        }

        private void _disconnected()
        {

            try
            {
                Disconnected?.Invoke();
                _Socket.Shutdown(SocketShutdown.Both);
            }
            catch { } // throws if client process has already closed
            _Socket.Close();
            _Socket = null;
        }

        //获取大端模式int值
        private int GetInt32BE(byte[] data, int index)
        {
            return (data[index] << 0x18) | (data[index + 1] << 0x10) | (data[index + 2] << 8) | (data[index + 3]);
        }
    }
}
