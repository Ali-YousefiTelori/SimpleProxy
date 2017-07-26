using SimpleProxyEncryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SimpleProxyServer
{
    public class ServerProxy
    {
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
            Thread thread = new Thread(() =>
            {
                try
                {
                    ClientConnector connector = new ClientConnector();


                    //connector.HandleTCPRequest(client);
                    //return;
                    var readerStream = new SecurityStream(client.GetStream());

                    byte[] readBytes = new byte[65536];
                    var readCount = readerStream.Read(readBytes, 0, readBytes.Length);
                    if (readCount == 0)
                    {
                        readerStream.Close();
                        return;
                    }
                    var hostAndPort = ClientConnector.GetHostAndPortNumber(readBytes, readCount);
                    if (hostAndPort.Port == 443)
                    {
                        connector.HandleTCPRequest(client, readBytes, readCount);
                        return;
                    }
                    readerStream.ReadTimeout = 20;
                    bool firstTimeRead = true;
                    while (client.Connected)
                    {
                        if (firstTimeRead)
                        {
                            firstTimeRead = false;
                            connector.Start(readerStream, readBytes, readCount);
                        }
                        else
                        {
                            connector.Write(readBytes, readCount);
                        }
                        readBytes = new byte[65536];
                        readCount = readerStream.Read(readBytes, 0, readBytes.Length);
                        if (readCount == 0)
                        {
                            Thread.Sleep(5000);
                            break;
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
