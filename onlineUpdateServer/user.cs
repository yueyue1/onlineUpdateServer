using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace onlineUpdateServer
{
    class user
    {
        public Socket workSocket;
        public static int bufferSize = 1024;
        public byte[] buffer = new byte[bufferSize];
    }
}
