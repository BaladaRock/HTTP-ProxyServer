using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProxyServer
{
    internal static class ProxyServer
    {
        private const string ProxyIP = "192.168.1.125";

        public static void Main()
        {
            TcpListener proxy = new TcpListener(IPAddress.Parse(ProxyIP), 8080);

            proxy.Start();

            while (true)
            {
                Console.Write("Waiting for web client... ");

                TcpClient client = proxy.AcceptTcpClient();
                Console.WriteLine("Connected!");

                NetworkStream browserStream = client.GetStream();
                string request = GetRequest(browserStream, client);

                if (CheckRequest(request))
                {
                    string[] requestDetails = request.Trim().Split('\n');
                    string hostHeader = requestDetails[1];
                    int lastPosition = hostHeader.LastIndexOf('\r');

                    string host = hostHeader.Substring(6, lastPosition - 6);
                    NetworkStream stream = new TcpClient(host, 80).GetStream();

                    SendRequest(stream, request);

                    byte[] response = GetResponse(stream);
                    Console.WriteLine($"Response from host: {response}");

                    SendResponse(client, response);
                }

                Console.Read();
            }
        }

        private static void SendResponse(TcpClient browserClient, byte[] response)
        {
            browserClient.Client.Send(response);
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
                message = Encoding.UTF8.GetString(buffer);
                Console.WriteLine("Request: " + message);
            }

            return message;
        }

        private static byte[] GetResponse(NetworkStream stream)
        {
            // Buffer to store the response bytes.
            byte[] recv = new byte[1000];

            // Read the first batch of the TcpServer response bytes.
            int bytes = stream.Read(recv, 0, recv.Length); //(**This receives the data using the byte method**)

            byte[] response = new byte[bytes];

            for (int i = 0; i < bytes; i++)
            {
                response[i] = recv[i];
            }

            return response;
        }

        private static void SendRequest(NetworkStream stream, string request)
        {
            stream.Write(Encoding.ASCII.GetBytes(request));
            Console.WriteLine($"Proxy has sent request to host: {request}");
        }
    }
}