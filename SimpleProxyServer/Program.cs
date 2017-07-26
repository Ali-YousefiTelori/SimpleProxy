using SimpleProxyEncryption;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SimpleProxyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerProxy server = new ServerProxy();
            server.Start(2525);
        }
    }
}
