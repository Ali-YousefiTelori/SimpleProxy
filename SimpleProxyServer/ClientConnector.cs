using SimpleProxyEncryption;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SimpleProxyServer
{
    public class ClientConnector : IDisposable
    {
        SecurityStream _clientStream;
        NetworkStream _serverStream;
        TcpClient _client;
        string Host { get; set; }
        int Port { get; set; }
        public void Start(SecurityStream stream, byte[] firstBytes, int readCount)
        {
            _clientStream = stream;
            var hostAndport = GetHostAndPortNumber(firstBytes, readCount);
            Debug.WriteLine("host:" + hostAndport.Host);
            Host = hostAndport.Host;
            Port = hostAndport.Port;
            _client = new TcpClient(hostAndport.Host, hostAndport.Port);
            _serverStream = _client.GetStream();
            Write(firstBytes, readCount);
            StartServerReader();
        }

        public static (int Port, string Host) GetHostAndPortNumber(byte[] bytes, int readCount)
        {
            int port = 0;
            var text = Encoding.ASCII.GetString(bytes.ToList().GetRange(0, readCount).ToArray());
            var host = text.Split(new string[] { " " }, StringSplitOptions.None)[1].Trim();
            Console.WriteLine(text);
            Debug.WriteLine(text);
            if (host.ToLower().Contains("http://"))
            {
                host = host.ToLower().Replace("http://", "");
                if (host.Contains("/"))
                    host = host.Split(new string[] { "/" }, StringSplitOptions.None)[0].Trim();
            }
            if (host.Contains("www."))
                host = host.Replace("www.", "");
            if (host.Contains(":"))
            {
                var newPort = host.Split(new string[] { ":" }, StringSplitOptions.None)[1].Trim();
                Debug.WriteLine("port:" + newPort);
                port = int.Parse(newPort);
                host = host.Split(new string[] { ":" }, StringSplitOptions.None)[0].Trim();
            }
            else
                port = 80;
            return (port, host);
        }

        void StartServerReader()
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    using (var readerStream = _serverStream)
                    {
                        while (_client.Connected)
                        {
                            byte[] readBytes = new byte[65536];
                            var readCount = readerStream.Read(readBytes, 0, readBytes.Length);
                            if (readCount == 0)
                            {
                                Thread.Sleep(5000);
                                break;
                            }
                            _clientStream.Write(readBytes, 0, readCount);
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

        public void Write(byte[] bytes, int writeCount)
        {
            _serverStream.Write(bytes, 0, writeCount);
        }

        /// <summary>
        /// Handles a TCP request.
        /// </summary>
        /// <param name="clientObject">The tcp client from the accepted connection.</param>
        public void HandleTCPRequest(TcpClient inClient, byte[] readBytes, int readCount)
        {
            TcpClient outClient = null;

            try
            {
                var clientStream = new SecurityStream(inClient.GetStream());
                // Read initial request.
                List<String> connectRequest = new List<string>();
                var text = Encoding.ASCII.GetString(readBytes.ToList().GetRange(0, readCount).ToArray());
                foreach (var line in text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    connectRequest.Add(line);
                }
                if (connectRequest.Count == 0)
                {
                    throw new Exception();
                }
                var hostAndPort = GetHostAndPortNumber(readBytes, readCount);

                // Connect to server
                outClient = new TcpClient(hostAndPort.Host, hostAndPort.Port);
                if (hostAndPort.Port == 443)
                {
                    // Send 200 Connection Established to Client
                    var bytes = Encoding.ASCII.GetBytes("HTTP/1.0 200 Connection established\r\n\r\n");
                    clientStream.Write(bytes, 0, bytes.Length);
                    clientStream.Flush();
                    Console.WriteLine("Established TCP connection for " + hostAndPort.Host + ":" + hostAndPort.Port);
                }
                else
                {
                    clientStream.Write(readBytes, 0, readCount);
                }
                Thread clientThread = new Thread(() => TunnelTCP(clientStream, new SecurityStream(outClient.GetStream()) { IsEnabled = false }));
                Thread serverThread = new Thread(() => TunnelTCP(new SecurityStream(outClient.GetStream()) { IsEnabled = false }, clientStream));

                clientThread.Start();
                serverThread.Start();
            }
            catch (Exception)
            {
                // Disconnent if connections still alive
                Console.WriteLine("Closing TCP connection.");
                try
                {
                    if (inClient.Connected)
                    {
                        inClient.Close();
                    }
                    if (outClient != null && outClient.Connected)
                    {
                        outClient.Close();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not close the tcp connection: ", e);
                }
            }
        }

        /// <summary>
        /// Tunnels a TCP connection.
        /// </summary>
        /// <param name="inClient">The client to read from.</param>
        /// <param name="outClient">The client to write to.</param>
        public void TunnelTCP(SecurityStream inStream, SecurityStream outStream)
        {
            try
            {
                using (inStream)
                {
                    using (outStream)
                    {
                        byte[] buffer = new byte[1024];
                        try
                        {
                            while (true)
                            {
                                var readCount = inStream.Read(buffer, 0, buffer.Length);
                                if (readCount == 0)
                                {
                                    Thread.Sleep(5000);
                                    break;
                                }
                                outStream.Write(buffer, 0, readCount);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("TCP connection error: ", e);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                inStream.Close();
                outStream.Close();
            }
        }

        public void Dispose()
        {
            _client.Close();
        }
    }

}
