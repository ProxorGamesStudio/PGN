using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using PGN.Data;

namespace PGN.General
{
    public class ClientHandler : Handler
    {
        private TcpClient tcpClient;
        private UdpClient udpClient;

        private NetworkStream stream;

        public event Action<NetData> onMessageRecieveTCP;
        public event Action<NetData> onMessageRecieveUDP;

        public event Action onConnect;
        public event Action onDisconnect;

        public void Connect()
        {
            tcpClient = new TcpClient(localAddressTCP);
            udpClient = new UdpClient(localAddressUDP);

            try
            {
                tcpClient.Connect(remoteAddressTCP);
                udpClient.Connect(remoteAddressUDP);
                stream = tcpClient.GetStream();

                byte[] data = NetData.GetBytesData(new NetData(0, user));
                stream.Write(data, 0, data.Length);
                udpClient.Send(data, data.Length);
                onConnect();
            }
            catch (Exception ex)
            {
                OnLogReceived(ex.Message);
                Disconnect();
            }
        }

        public void SendMessageTCP(NetData message)
        {
            byte[] data = NetData.GetBytesData(message);
            stream.Write(data, 0, data.Length);
        }

        public void ReceiveMessageTCP()
        {
            try
            {
                byte[] data = new byte[1024];

                do
                {
                    stream.Read(data, 0, data.Length);
                    onMessageRecieveTCP(NetData.RecoverBytes(data));
                }
                while (stream.DataAvailable);

            }
            catch
            {
                Disconnect();
            }

        }


        public void SendMessageUDP(NetData message)
        {
            byte[] data = NetData.GetBytesData(message);
            udpClient.Send(data, data.Length);
        }

        public void ReceiveMessageUDP()
        {
            try
            {
                byte[] data = new byte[1024];
                data = udpClient.Receive(ref remoteAddressUDP);
                onMessageRecieveUDP(NetData.RecoverBytes(data));
            }
            catch
            {
                Disconnect();
            }

        }

        public void Disconnect()
        {
            if (stream != null)
                stream.Close();
            if (tcpClient != null)
                tcpClient.Close();
            if (udpClient != null)
                udpClient.Close();
            onDisconnect();
        }
    }
}

