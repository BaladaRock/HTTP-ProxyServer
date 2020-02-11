using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProxyServer
{
    internal static class ProxyServer
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

                string response = string.Empty;

                NetworkStream browserStream = client.GetStream();
                string message = GetRequest(browserStream, client);

                if (CheckRequest(message))
                {
                    string[] requestDetails = message.Trim().Split('\n');
                    string hostHeader = requestDetails[1];
                    int lastPosition = hostHeader.LastIndexOf('\r');

                    string host = hostHeader.Substring(6, lastPosition - 6);

                    NetworkStream stream = new TcpClient(host, 80).GetStream();

                    SendRequest(stream, host);

                    response += GetResponse(stream);
                    Console.WriteLine($"Response from host: {response}");

                    

                    SendResponse(client.GetStream(), response);
                }

                Console.Read();
            }
        }

        private static void SendResponse(NetworkStream stream, string response)
        {
            stream.Write(Encoding.ASCII.GetBytes(response));
            Console.WriteLine($"Proxy has sent host response back to browser: {response}");
        }

        private static bool CheckRequest(string request)
        {
            return request?.StartsWith("GET http://") == true;
        }

        private static string GetRequest(NetworkStream stream, TcpClient client)
        {
            string message = null;
            byte[] buffer = null;

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

        private static string GetResponse(NetworkStream stream)
        {
            byte[] buffer = new byte[256];
            int bytes = stream.Read(buffer, 0, buffer.Length);

            return Encoding.ASCII.GetString(buffer, 0, bytes);
        }

        private static void SendRequest(NetworkStream stream, string host)
        {
            stream.Write(Encoding.ASCII.GetBytes(host));
            Console.WriteLine($"Proxy has sent request to host: {host}");
        }
    }
}