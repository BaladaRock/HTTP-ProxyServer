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
        private const string ContentLength = "Content-Length: ";
        private const string EmptyLine = "\r\n\r\n";
        private const string NewLine = "\r\n";

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
            byte[] buffer = null;

            /* var httpParser = new HttpParser(buffer);

             while (!httpParser.Contains(Encoding.UTF8.GetBytes("Transfer-Encoding:\nchunked")))
             {
                 stream.Read(buffer, 0, buffer.Length);
                 httpParser = new HttpParser(buffer);

             }

             buffer = null;
             var streamReader = new StreamReader(stream);
             buffer = Encoding.UTF8.GetBytes(streamReader.ReadLine());*/

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
            byte[] emptyLine = Encoding.UTF8.GetBytes(EmptyLine);
            byte[] newLine = Encoding.UTF8.GetBytes(NewLine);
            byte[] contentHeader = Encoding.UTF8.GetBytes(ContentLength);
            int bodyLength = 0;

            byte[] buffer = new byte[512];
            int readFromStream = stream.Read(buffer, 0, buffer.Length);

            #region Handle Content-Length
            var readHeaders = new HttpReader(buffer, 512, NewLine);

            var readLine = readHeaders.ReadLine();

            while (!readLine.SequenceEqual(emptyLine))
            {
                var lineChecker = new HttpParser(readLine);

                if (lineChecker.Contains(contentHeader))
                {
                    int headerTitle = lineChecker.GetPosition(contentHeader);

                    bodyLength = Convert.ToInt32(Encoding.UTF8.GetString(
                        readLine.Skip(headerTitle)
                            .TakeWhile(bytes => bytes != '\r')
                                .ToArray()));
                }
            }


            /*  if ()
              {
                  HandleContentLength(checkHeaders, stream);


              }

              if (checkHeaders.Contains(emptyLine))
              {
                  int bytesToSend = checkHeaders.GetPosition(emptyLine);
                  client.GetStream().Write(buffer, 0, bytesToSend);

                  byte[] remainingBytes = buffer.Skip(bytesToSend)
                      .Take(buffer.Length - bytesToSend)
                    .ToArray();
                  HandleChunkedBody(remainingBytes, stream);
              }*/

            #endregion
        }

        private void HandleContentLength(HttpParser checkHeaders, NetworkStream stream)
        {

        }

        private void HandleChunkedBody(byte[] bytesToSend, NetworkStream stream)
        {
            byte[] buffer = null;
            int remainderLength = bytesToSend.Length;

            HttpReader bodyReader = new HttpReader(bytesToSend, remainderLength, NewLine);


            byte[] bytesToRead = bodyReader.ReadLine();
            if (bodyReader.IsChunkComplete(bytesToRead))
            {
                buffer = bodyReader.ReadBytes(Encoding.UTF8.GetString(bytesToRead));
                byte[] newBuffer = bytesToSend.Skip(Convert.ToInt32(bytesToRead)).ToArray();

                HttpReader newReader = new HttpReader(newBuffer, newBuffer.Length, NewLine);
                // newReader.ReadLine() =
            }

            while (bodyReader.ReadLine() != Encoding.UTF8.GetBytes("0"))
            {

            }

            byte[] getChunkSize = bodyReader.ReadLine();

            bodyReader.ReadBytes(Encoding.UTF8.GetString(getChunkSize));

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