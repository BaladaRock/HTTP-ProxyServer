using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProxyServer
{
    internal static class ProxyServer
    {
        private const string LocalHost = "192.168.1.25";

        public static void Main()
        {
            TcpListener proxy = new TcpListener(IPAddress.Parse(LocalHost), 8080);

            proxy.Start();

            while (true)
            {
                Console.Write("Waiting for web client... ");

                TcpClient client = proxy.AcceptTcpClient();
                Console.WriteLine("Connected!");

                string message = GetRequest(client);

                if (CheckRequest(message))
                {
                    string[] requestDetails = message.Trim().Split('\n');
                    string hostHeader = requestDetails[1];
                    int lastPosition = hostHeader.LastIndexOf('\r');

                    string host = hostHeader.Substring(6, lastPosition - 6);

                    TcpClient proxyClient = new TcpClient(host, 80);

                    SendRequestToServer(proxyClient, host);

                    GetResponse(client);
                }

                // Shutdown and end connection
                client.Close();
                Console.Read();
            }
        }

        private static bool CheckRequest(string request)
        {
            return request?.StartsWith("GET http://") == true;
        }

        private static string GetRequest(TcpClient client)
        {
            string message = null;
            byte[] buffer = null;

            NetworkStream stream = client.GetStream();

            if (client.ReceiveBufferSize > 0)
            {
                buffer = new byte[client.ReceiveBufferSize];
                stream.Read(buffer, 0, client.ReceiveBufferSize);

                //The received request
                message += Encoding.ASCII.GetString(buffer);
                Console.WriteLine("Request: " + message);
            }

            return message;
        }

        private static void GetResponse(TcpClient client)
        {
            byte[] receivedMessage = new byte[client.ReceiveBufferSize];

            int size = client.Client.Receive(receivedMessage);
            string response = Encoding.UTF8.GetString(receivedMessage, 0, size);

            Console.WriteLine($"Response from host: {response}");
        }

        private static void SendRequestToServer(TcpClient client, string host)
        {
            client.Client.Send(Encoding.ASCII.GetBytes(host));
            Console.WriteLine($"Proxy has sent request to host: {host}");
        }
    }
}