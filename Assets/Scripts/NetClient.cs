using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Summer;
using System.Net;
using System.Net.Sockets;
using Summer.Network;
using System;
using Google.Protobuf;
using Kaiyun;

/// <summary>
/// 网络客户端，其实就是基于Connection的封装
/// </summary>
public class NetClient
{

    private static Connection connection = null;

    public static void Send(IMessage message)
    {
        if (connection != null)
        {
            connection.Send(message);
        }
    }


    /// <summary>
    /// 连接到服务器
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    public static void ConnectToServer(string host, int port)
    {
        //服务器终端
        IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(host), port);
        Socket socket = new Socket(AddressFamily.InterNetwork,
                                    SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(ipe);
        Debug.Log("连接到服务端");

        //在给Socket赋值完再穿给Connection，因为Connection内部没有绑定相关
        connection = new Connection(socket);
        connection.OnDisconnected += OnDisconnected;
        //启动消息分发器
        MessageRouter.Instance.Start(1);
    }

    //连接断开，用于监听Connection断开
    private static void OnDisconnected(Connection conn)
    {
        Debug.Log("与服务器断开");
        Kaiyun.Event.FireOut("OnDisconnected");
    }

    /// <summary>
    /// 关闭网络客户端
    /// </summary>
    public static void Close()
    {
        try { connection?.Close(); } catch { }
    }

}
