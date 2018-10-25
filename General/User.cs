using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PGN.General
{
    [Serializable]
    public class User
    {     
        public string ID;
        public string name;

        public User()
        {
            ID = Dns.GetHostName() + "@" + DateTime.Now.ToString(); 
        }
    }
}
