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
        private TcpClient httpsServer;

        public TlsHandler(TcpClient browser)
        {
            this.browser = browser;
        }

        public void StartHandshake(string host, int port)
        {
            httpsServer = new TcpClient(host, port);

            Thread clientThread = new Thread(() => CreateTLSTunnel(browser, httpsServer));
            clientThread.Start();

            Thread serverThread = new Thread(() => CreateTLSTunnel(httpsServer,browser));
            serverThread.Start();
        }

        private void CreateTLSTunnel(TcpClient client, TcpClient server)
        {
            byte[] buffer = new byte[2048];

            try
            {
                NetworkStream browserStream = client.GetStream();
                NetworkStream serverStream = server.GetStream();

                while (client.Connected && server.Connected)
                {
                    if (browserStream.DataAvailable)
                    {
                        int read = browserStream.Read(buffer, 0, buffer.Length);
                        serverStream.Write(buffer, 0, read);

                        string message = Encoding.UTF8.GetString(buffer.Take(read).ToArray());
                        Console.WriteLine($"Encoded message:{message}");
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
    }
}