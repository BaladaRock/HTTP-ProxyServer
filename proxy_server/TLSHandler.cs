using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ProxyServer
{
    public class TlsHandler
    {
        private readonly object baton;
        private readonly TcpClient browser;
        private TcpClient httpsServer;

        public TlsHandler(TcpClient browser, object baton = null)
        {
            this.browser = browser;
            this.baton = baton;
        }

        public void StartHandshake(string host, int port)
        {
            try
            {
                httpsServer = new TcpClient(host, port);
                Thread clientThread = new Thread(() => CreateTLSTunnel(browser, httpsServer));
                clientThread.Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Exception: {exception}");
            }
        }

        internal byte[] GetFailureResponse()
        {
            return Headers.FailureStatusMessage;
        }

        internal byte[] GetSuccessResponse()
        {
            return Headers.SuccesStatusMessage;
        }

        private static void WriteMessage(byte[] buffer, int read)
        {
            string message = Encoding.UTF8.GetString(buffer.Take(read).ToArray());
            Console.WriteLine($"Encoded message:{message}");
        }

        private void CreateTLSTunnel(TcpClient client, TcpClient server)
        {
            try
            {
                lock (baton)
                {
                    byte[] sessionBuffer = new byte[2048];
                    NetworkStream browserStream = client.GetStream();
                    NetworkStream serverStream = server.GetStream();

                    if (!server.Connected)
                    {
                        SendStatusMessage(browserStream, Headers.FailureStatusMessage);
                    }
                    else
                    {
                        SendStatusMessage(browserStream, Headers.SuccesStatusMessage);
                    }
                    browserStream.Flush();
                    serverStream.Flush();

                    while (client.Connected && server.Connected)
                    {
                        if (browserStream.DataAvailable)
                        {
                            int browserRead = browserStream.Read(sessionBuffer, 0, sessionBuffer.Length);
                            serverStream.Write(sessionBuffer, 0, browserRead);
                            WriteMessage(sessionBuffer, browserRead);
                        }

                        if (serverStream.DataAvailable)
                        {
                            int serverRead = serverStream.Read(sessionBuffer, 0, sessionBuffer.Length);
                            browserStream.Write(sessionBuffer, 0, serverRead);
                            WriteMessage(sessionBuffer, serverRead);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (client.Connected)
                {
                    client.Close();
                }

                if (server.Connected)
                {
                    server.Close();
                }

                Console.WriteLine($"Connection exception: {exception}");
            }
        }

        private void SendStatusMessage(NetworkStream browserStream, byte[] message)
        {
            browserStream.Write(message, 0, message.Length);
            Console.WriteLine($"Encoding.UTF8.GetString(message)");
        }
    }
}