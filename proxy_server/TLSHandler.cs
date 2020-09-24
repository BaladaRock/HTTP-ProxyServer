using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ProxyServer
{
    public class TlsHandler
    {
        private readonly TcpClient browser;
        private readonly byte[] sessionBuffer;
        private TcpClient httpsServer;

        public TlsHandler(TcpClient browser)
        {
            this.browser = browser;
            sessionBuffer = new byte[2048];
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

        private void CheckAndCloseConnections(TcpClient server, TcpClient client)
        {
            if (server.Connected)
            {
                server.Close();
            }

            if (client.Connected)
            {
                client.Close();
            }
        }

        private void CloseConnections()
        {
            httpsServer.Close();
            browser.Close();
        }

        private void CreateTLSTunnel(TcpClient client, TcpClient server)
        {
            try
            {
                NetworkStream browserStream = client.GetStream();
                NetworkStream serverStream = server.GetStream();

                SendHandshakeStatus(browserStream, server);

                while (client.Connected && server.Connected)
                {
                    if (browserStream.DataAvailable)
                    {
                        GetAndSendMessage(browserStream, serverStream);
                    }

                    if (serverStream.DataAvailable)
                    {
                        GetAndSendMessage(serverStream, browserStream);
                    }
                }

                CloseConnections();
            }
            catch (Exception exception)
            {
                CheckAndCloseConnections(server, client);
                Console.WriteLine($"Connection exception: {exception}");
            }
        }

        private void GetAndSendMessage(NetworkStream sender, NetworkStream receiver)
        {
            int senderRead = sender.Read(sessionBuffer, 0, sessionBuffer.Length);
            receiver.Write(sessionBuffer, 0, senderRead);
            WriteMessage(sessionBuffer, senderRead);
        }

        private void SendHandshakeStatus(NetworkStream browserStream, TcpClient server)
        {
            if (server.Connected)
            {
                SendStatusMessage(browserStream, Headers.SuccesStatusMessage);
            }
            else
            {
                SendStatusMessage(browserStream, Headers.FailureStatusMessage);
            }
        }

        private void SendStatusMessage(NetworkStream browserStream, byte[] message)
        {
            browserStream.Write(message, 0, message.Length);
        }
    }
}