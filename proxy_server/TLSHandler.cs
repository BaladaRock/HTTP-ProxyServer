using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ProxyServer
{
    public class TlsHandler
    {
        private readonly RequestReader reader;
        private readonly TcpClient browser;
        private TcpClient httpsServer;

        public TlsHandler(TcpClient browser, RequestReader reader)
        {
            this.browser = browser;
            this.reader = reader;
        }

        public void StartHandshake(string host, int port)
        {
            httpsServer = new TcpClient(host, port);

            Thread clientThread = new Thread(() => CreateTLSTunnel(browser, httpsServer));
            clientThread.Start();
        }

        private void CreateTLSTunnel(TcpClient client, TcpClient server)
        {
            byte[] buffer = new byte[2048];

            try
            {
                NetworkStream browserStream = client.GetStream();
                NetworkStream serverStream = server.GetStream();
                byte[] request = Encoding.UTF8.GetBytes(reader.Request);
                serverStream.Write(request);
                WriteMessage(request, request.Length);

                while (client.Connected && server.Connected)
                {
                    int serverRead = serverStream.Read(buffer, 0, buffer.Length);
                    browserStream.Write(buffer, 0, serverRead);
                    WriteMessage(buffer, serverRead);

                    int browserRead = browserStream.Read(buffer, 0, buffer.Length);
                    serverStream.Write(buffer, 0, browserRead);
                    WriteMessage(buffer, browserRead);
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

        private static void WriteMessage(byte[] buffer, int read)
        {
            string message = Encoding.UTF8.GetString(buffer.Take(read).ToArray());
            Console.WriteLine($"Encoded message:{message}");
        }
    }
}