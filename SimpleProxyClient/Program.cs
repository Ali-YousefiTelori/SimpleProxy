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

namespace SimpleProxyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientProxy clientProxy = new ClientProxy("localhost", 2525);
            clientProxy.Start(4545);
        }
    }
}
