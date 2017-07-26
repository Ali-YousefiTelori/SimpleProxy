using SimpleProxyEncryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SimpleProxyClient
{
    public class ClientProxy
    {
        public ClientProxy(string serverAddress,int serverPort)
        {
            ServerIpAddress = serverAddress;
            ServerPort = serverPort;
        }

        public string ServerIpAddress { get; set; }
        public int ServerPort { get; set; }

        public void Start(int port)
        {
            try
            {
                var _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();

                while (true)
                {
                    Console.WriteLine("wait for new client");
                    AcceptClient(_listener.AcceptTcpClient());
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                Console.WriteLine("SERVER ERROR");
                Console.ReadKey();
            }
        }


        public void AcceptClient(TcpClient client)
        {

            try
            {
                TcpClient server = new TcpClient(ServerIpAddress, ServerPort);
                DuplexReadAndWrite(new SecurityStream(client.GetStream()) { IsEnabled = false }, new SecurityStream(server.GetStream()));
                DuplexReadAndWrite(new SecurityStream(server.GetStream()), new SecurityStream(client.GetStream()) { IsEnabled = false });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void DuplexReadAndWrite(SecurityStream readerStream, SecurityStream writerStream)
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    using (readerStream)
                    {
                        using (writerStream)
                        {
                            while (true)
                            {
                                byte[] readBytes = new byte[65536];
                                var readCount = readerStream.Read(readBytes, 0, readBytes.Length);
                                if (readCount == 0)
                                {
                                    Thread.Sleep(5000);
                                    break;
                                }
                                writerStream.Write(readBytes, 0, readCount);
                            }
                        }

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
            thread.Start();
        }
    }
}
