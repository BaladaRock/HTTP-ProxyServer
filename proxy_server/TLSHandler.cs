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
        }

        internal byte[] GetSuccessResponse()
        {
            return Headers.SuccesStatusMessage;
        }

        internal byte[] GetFailureResponse()
        {
            return Headers.FailureStatusMessage;
        }

        private static void WriteMessage(byte[] buffer, int read)
        {
            string message = Encoding.UTF8.GetString(buffer.Take(read).ToArray());
            Console.WriteLine($"Encoded message:{message}");
        }

        private void CreateTLSTunnel(TcpClient client, TcpClient server)
        {
            byte[] buffer = new byte[2048];

            try
            {
                NetworkStream browserStream = client.GetStream();
                NetworkStream serverStream = server.GetStream();

                if (!server.Connected)
                {
                    browserStream.Write(GetFailureResponse());
                    //serverStream.Flush();
                }

                while (client.Connected && server.Connected)
                {
                    browserStream.Write(GetSuccessResponse());

                    if (browserStream.DataAvailable)
                    {
                        int browserRead = browserStream.Read(buffer, 0, buffer.Length);
                        serverStream.Write(buffer, 0, browserRead);
                        WriteMessage(buffer, browserRead);
                    }

                    if (serverStream.DataAvailable)
                    {
                        int serverRead = serverStream.Read(buffer, 0, buffer.Length);
                        browserStream.Write(buffer, 0, serverRead);
                        WriteMessage(buffer, serverRead);
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