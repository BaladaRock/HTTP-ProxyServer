using ProxyHTTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProxyServer
{
    public class ProxyServer
    {
        private readonly string proxyIP;

        public ProxyServer(string proxyIP)
        {
            this.proxyIP = proxyIP;
        }

        public void StartProxy()
        {
            TcpListener proxy = new TcpListener(IPAddress.Parse(proxyIP), 8080);

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

                    HandleResponseHeaders(client, stream);
                    /*Console.WriteLine($"Response from host: {Encoding.UTF8.GetString(response)}");

                    SendResponse(client, response);*/
                }
            }
        }

        private string GetHost(string request)
        {
            string[] requestDetails = request.Trim().Split('\n');
            string hostHeader = requestDetails[1];
            int lastPosition = hostHeader.LastIndexOf('\r');

            return hostHeader.Substring(6, lastPosition - 6);
        }

        private bool CheckRequest(string request)
        {
            return request?.StartsWith("GET http://") == true;
        }

        private string GetRequest(NetworkStream stream, TcpClient client)
        {
            string message = null;
            byte[] buffer = new byte[512];
            stream.Read(buffer, 0, buffer.Length);

            var httpParser = new HttpParser(buffer);

            while (!httpParser.Contains(Encoding.UTF8.GetBytes("Transfer-Encoding:\nchunked")))
            {
                stream.Read(buffer, 0, buffer.Length);
                httpParser = new HttpParser(buffer);

            }

            buffer = null;
            var streamReader = new StreamReader(stream);
            buffer = Encoding.UTF8.GetBytes(streamReader.ReadLine());

            if (client.ReceiveBufferSize > 0)
            {
                buffer = new byte[client.ReceiveBufferSize];
                stream.Read(buffer, 0, client.ReceiveBufferSize);

                message = Encoding.UTF8.GetString(buffer);
                Console.WriteLine("Request: " + message);
            }

            return message;
        }

        private void HandleResponseHeaders(TcpClient client, NetworkStream stream)
        {
            byte[] emptyLine = Encoding.UTF8.GetBytes("\r\n\r\n");

            byte[] buffer = new byte[512];
            int readFromStream = stream.Read(buffer, 0, buffer.Length);

            var checkHeaders = new HttpParser(buffer);

            if (checkHeaders.Contains(emptyLine))
            {
                int bytesToSend = checkHeaders.GetPosition(emptyLine);
                client.GetStream().Write(buffer, 0, bytesToSend);

                byte[] remainingBytes = buffer.Skip(bytesToSend)
                    .Take(buffer.Length - bytesToSend)
                  .ToArray();
                HandleResponseBody(remainingBytes, client, stream);
            }

            /*byte[] recv = new byte[1024];
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

            return dataRead;*/
        }

        private void HandleResponseBody(byte[] remainingBytes, TcpClient client, NetworkStream stream)
        {
            throw new NotImplementedException();
        }

        private void SendRequest(NetworkStream stream, string request)
        {
            stream.Write(Encoding.UTF8.GetBytes(request));
            Console.WriteLine($"Proxy has sent request to host: {request}");
        }

        private void SendResponse(TcpClient browserClient, byte[] response)
        {
            browserClient.Client.Send(response);

            string textResponse = Encoding.UTF8.GetString(response);
            Console.WriteLine($"Proxy has sent host response back to browser: {textResponse}");
        }

    }
}