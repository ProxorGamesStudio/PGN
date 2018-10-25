using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using PGN.Data;

namespace PGN.General
{
    public class NetUser
    {

        protected internal User user            { get; set; }
        protected internal NetworkStream stream { get; private set; }

        public TcpClient tcpClient;
        public UdpClient udpClient;

        private ServerHandler server;

        public static event Action<NetData> onTcpMessageHandleCallback;
        public static event Action<NetData> onUdpMessageHandleCallback;

        public static event Action<User> onUserConnectedTCP;
        public static event Action<User> onUserDisconnectedTCP;

        public static event Action<User> onUserConnectedUDP;
        public static event Action<User> onUserDisconnectedUDP;

        internal IPAddress adress;

        private bool tcpConnected = false;
        private bool udpConnected = false;

        public NetUser(TcpClient tcpClient, ServerHandler server, IPAddress adress)
        {
            this.tcpClient = tcpClient;
            this.server = server;
            this.adress = adress;
            this.server.AddConnection(this);

            stream = tcpClient.GetStream();
        }

        public NetUser(UdpClient udpClient, ServerHandler server, IPAddress adress)
        {
            this.udpClient = udpClient;
            this.server = server;
            this.adress = adress;
            this.server.AddConnection(this);
        }

        public void RecieveTCP()
        {
            while (true)
            {
                try
                {
                    GetMessageTCP();
                }
                catch (Exception e)
                {
                    onUserDisconnectedTCP(user);
                    onUserDisconnectedUDP(user);
                    server.RemoveConnection(this);
                    Close();
                    break;
                }
            }
        }

        public void RecieveUDP(byte[] bytes)
        {
            GetMessageUDP(bytes);
        }

        private void GetMessageUDP(byte[] bytes)
        {
            NetData message = NetData.RecoverBytes(bytes);
            if (user == null)
            {
                user = message.sender;
                onUserConnectedUDP(user);
                udpConnected = true;
            }
            else if (!udpConnected)
            {
                onUserConnectedUDP(user);
                udpConnected = true;
            }

            onUdpMessageHandleCallback(message);
            server.BroadcastMessageUDP(bytes, message.sender);
        }

        private void GetMessageTCP()
        {
            byte[] bytes = new byte[1024];
            
            do
            {
                stream.Read(bytes, 0, bytes.Length);
                NetData message = NetData.RecoverBytes(bytes);
                if (user == null)
                {
                    user = message.sender;
                    onUserConnectedTCP(user);
                    tcpConnected = true;
                }
                else if(!tcpConnected)
                {
                    onUserConnectedTCP(user);
                    tcpConnected = true;
                }

                server.BroadcastMessageTCP(bytes, message.sender);
                onTcpMessageHandleCallback(message);
            }
            while (stream.DataAvailable);

        }

        protected internal void Close()
        {
            if (stream != null)
                stream.Close();
            if (tcpClient != null)
                tcpClient.Close();
        }

    }
}

