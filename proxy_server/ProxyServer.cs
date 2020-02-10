using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProxyServer
{
    internal class ProxyServer
    {
        private const string LocalHost = "192.168.1.125";

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
                    SendRequestToServer(message);
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

        private static void SendRequestToServer(string message)
        {
            string[] requestDetails = message.Trim().Split('\n');

            int hostIndex = message.IndexOf("\nHost: ");
            string host = message.Substring(hostIndex + 7, requestDetails[1].Length - 7);

            TcpListener hostServer = new TcpListener(IPAddress.Parse(LocalHost), 8081);
            TcpClient client = new TcpClient($"http://{host}", 8081);

            Console.Write($"Proxy has sent request to host: {host}");

            Console.Read();
        }
    }
}