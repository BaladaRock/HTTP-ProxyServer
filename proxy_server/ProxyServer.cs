using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                    string host = GetHost(request);
                    NetworkStream stream = new TcpClient(host, 80).GetStream();

                    SendRequest(stream, request);

                    byte[] response = GetResponse(stream);
                    Console.WriteLine($"Response from host: {Encoding.UTF8.GetString(response)}");

                    SendResponse(client, response);
                }
            }
        }

        private static string GetHost(string request)
        {
            string[] requestDetails = request.Trim().Split('\n');
            string hostHeader = requestDetails[1];
            int lastPosition = hostHeader.LastIndexOf('\r');

            return hostHeader.Substring(6, lastPosition - 6);
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

                message = Encoding.UTF8.GetString(buffer);
                Console.WriteLine("Request: " + message);
            }

            return message;
        }

        private static byte[] GetResponse(NetworkStream stream)
        {
            byte[] recv = new byte[1024];
            byte[] dataRead;
            int bytesRead = 0;

            using (MemoryStream readStream = new MemoryStream())
            {
                while ((bytesRead = stream.Read(recv, 0, recv.Length)) > 0)
                {
                    readStream.Write(recv, 0, bytesRead);
                }

                dataRead = readStream.ToArray();
            }

            return dataRead;
        }

        private static void SendRequest(NetworkStream stream, string request)
        {
            stream.Write(Encoding.UTF8.GetBytes(request));
            Console.WriteLine($"Proxy has sent request to host: {request}");
        }

        private static void SendResponse(TcpClient browserClient, byte[] response)
        {
            string textResponse = Encoding.UTF8.GetString(response);

            //CheckForChunked(textResponse);
            browserClient.Client.Send(response);

            Console.WriteLine($"Proxy has sent host response back to browser: {textResponse}");
        }

        private static void CheckForChunked(string response)
        {

        }
    }
}