using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

using PGN.Data;

namespace PGN.General
{
    public class ServerHandler : Handler
    {

        private static ManualResetEvent allDoneTCP = new ManualResetEvent(false);
        private static ManualResetEvent allDoneUDP = new ManualResetEvent(false);

        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        private Hashtable clients = new Hashtable();

        protected internal void AddConnection(NetUser client)
        {
            clients.Add(client.adress, client);
        }
        protected internal void RemoveConnection(NetUser user)
        {
            if (clients.Contains(user.adress))
                clients.Remove(user.adress);
        }

        public void Start()
        {
            tcpListener = new TcpListener(localAddressTCP);
            udpListener = new UdpClient(localAddressUDP);
            tcpListener.Start();
            OnLogReceived("Server was created.");
        }

        public void ListenTCP()
        {
            try
            {
                allDoneTCP.Reset();
                tcpListener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), tcpListener);
                allDoneTCP.WaitOne();
            }
            catch (Exception ex)
            {
                OnLogReceived(ex.ToString());
                Stop();
            }
        }

        public void ListenUDP()
        {
            try
            {
                allDoneUDP.Reset();
                udpListener.BeginReceive(ReceiveCallback, udpListener);
                allDoneUDP.WaitOne();
            }
            catch (Exception ex)
            {

            }
        }

        protected internal void AcceptCallback(IAsyncResult ar)
        {
            allDoneTCP.Set();

            TcpClient tcpClient = (ar.AsyncState as TcpListener).EndAcceptTcpClient(ar);
            NetUser client = null;
            if (clients.Contains((tcpClient.Client.RemoteEndPoint as IPEndPoint).Address))
            {
                client = clients[(tcpClient.Client.RemoteEndPoint as IPEndPoint).Address] as NetUser;
                client.tcpClient = tcpClient;
            }
            else
                client = new NetUser(tcpClient, this, (tcpClient.Client.RemoteEndPoint as IPEndPoint).Address);

            client.RecieveTCP();
        }

        protected internal void ReceiveCallback(IAsyncResult ar)
        {
            allDoneUDP.Set();

            byte[] bytes = new byte[1024];

            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 8001);

            try
            {
                do
                {
                    bytes = udpListener.EndReceive(ar, ref iPEndPoint);
                }
                while ((ar.AsyncState as UdpClient).Available > 0);
               
                NetUser client = null;
                if (clients.Contains(iPEndPoint.Address))
                {
                    client = clients[iPEndPoint.Address] as NetUser;
                    client.udpClient = udpListener;
                }
                else
                    client = new NetUser(udpListener, this, iPEndPoint.Address);

                client.RecieveUDP(bytes);
            }
            catch(Exception e)
            {

            }
        }

        protected internal void testSend(IAsyncResult ar)
        {
            udpListener.EndSend(ar);
        }

        protected internal void BroadcastMessageTCP(byte[] data, User sender)
        {
            foreach(IPAddress key in clients.Keys)
                if((clients[key] as NetUser).user.ID != sender.ID)
                    (clients[key] as NetUser).stream.Write(data, 0, data.Length);
        }

        protected internal void BroadcastMessageUDP(byte[] data, User sender)
        {
            foreach (IPAddress key in clients.Keys)
                if ((clients[key] as NetUser).user.ID != sender.ID)
                    (clients[key] as NetUser).udpClient.BeginSend(data, data.Length, new IPEndPoint((clients[key] as NetUser).adress, 8001), testSend, (clients[key] as NetUser).udpClient);
        }

        protected internal void BroadcastMessageCallback(IAsyncResult ar)
        {
            UdpClient client = ar.AsyncState as UdpClient;
            client.EndSend(ar);
        }

        public void Stop()
        {
            tcpListener.Stop();

            for (int i = 0; i < clients.Count; i++)
            {
                (clients[i] as NetUser).Close();
            }
            Environment.Exit(0);
        }
    }
}
