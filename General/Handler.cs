using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using PGN.Data;

namespace PGN.General
{
    public class Handler
    {
        public static event Action<string> onLogReceived;

        public static readonly float version = 1.0f;

        protected static IPEndPoint localAddressTCP;
        protected static IPEndPoint localAddressUDP;

        protected static IPEndPoint remoteAddressTCP;
        protected static IPEndPoint remoteAddressUDP;

        protected Socket TCP_socket;

        protected static User user;

        public void SetLocalAdressTCP(string ip, int port)
        {
            localAddressTCP = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public void SetLocalAdressUDP(string ip, int port)
        {
            localAddressUDP = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public void SetRemoteAdressTCP(string ip, int port)
        {
            remoteAddressTCP = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public void SetRemoteAdressUDP(string ip, int port)
        {
            remoteAddressUDP = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public void RegistrUser(User _user)
        {
            user = _user;
            OnLogReceived("You registr new user - " + _user.ID);
        }

        public static void OnLogReceived(string text)
        {
            onLogReceived("PGN LOG: " + text);
        }

    }
}
