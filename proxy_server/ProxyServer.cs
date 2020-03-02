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
        private const string ChunkedTransfer = "Transfer-Encoding: ";
        private const string Chunked = "chunked";

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

            if (client.ReceiveBufferSize > 0)
            {
                buffer = new byte[client.ReceiveBufferSize];
                stream.Read(buffer, 0, client.ReceiveBufferSize);

                message = Encoding.UTF8.GetString(buffer);
                Console.WriteLine("Request: " + message);
            }

            return message;
        }

        private void HandleResponseHeaders(TcpClient browser, NetworkStream stream)
        {
            try
            {

                byte[] newLine = Encoding.UTF8.GetBytes(NewLine);
                byte[] emptyLine = Encoding.UTF8.GetBytes(EmptyLine);
                byte[] contentHeader = Encoding.UTF8.GetBytes(ContentLength);
                byte[] chunkedHeader = Encoding.UTF8.GetBytes(ChunkedTransfer);
                byte[] chunked = Encoding.UTF8.GetBytes(Chunked);
                byte[] buffer = new byte[512];
                bool isChunked = false;
                string bodyLength = null;

                int readFromStream = 0;

                if (stream.CanRead)
                {
                    readFromStream = stream.Read(buffer, 0, buffer.Length);
                }

                var readHeaders = new HttpReader(buffer, readFromStream, NewLine);

                byte[] readLine = readHeaders.ReadLine();
                Console.WriteLine(Encoding.UTF8.GetString(readLine));
                int countHeaderBytes = readLine.Length;

                while (!readLine.SequenceEqual(emptyLine))
                {
                    var lineChecker = new HttpParser(readLine);

                    if (lineChecker.Contains(contentHeader))
                    {
                        int headerTitle = lineChecker.GetPosition(contentHeader);

                        bodyLength = Encoding.UTF8.GetString(
                            readLine.Skip(headerTitle)
                                .TakeWhile(bytes => bytes != '\r')
                                    .ToArray());
                    }

                    if (lineChecker.Contains(chunkedHeader) && lineChecker.Contains(chunked))
                    {
                        isChunked = true;
                    }

                    readLine = readHeaders.ReadLine();

                    string convertedLine = Encoding.UTF8.GetString(readLine);
                    Console.WriteLine(convertedLine);

                    countHeaderBytes += convertedLine == EmptyLine
                        ? readLine.Length
                        : readLine.Length + 2;
                }

                #region Handle Transfered-Encoding
                if (isChunked)
                {
                    HandleChunkedEncoding(browser, stream, buffer, countHeaderBytes);
                }


                #endregion


                #region Handle Content-Length

                int bodyBytes = readFromStream - countHeaderBytes;

                SendResponse(browser, buffer.Take(readFromStream).ToArray());
                int totalbodyLength = Convert.ToInt32(bodyLength);

                if (totalbodyLength > bodyBytes)
                {
                    int remainigBytes = totalbodyLength - bodyBytes;

                    while (remainigBytes > 512)
                    {
                        readFromStream = stream.Read(buffer, 0, buffer.Length);
                        remainigBytes -= readFromStream;
                        SendResponse(browser, buffer.Take(readFromStream).ToArray());
                    }

                    readFromStream = stream.Read(buffer, 0, remainigBytes);
                    SendResponse(browser, buffer.Take(readFromStream).ToArray());
                }

                #endregion

                browser.Close();
            }
            catch (Exception exception)
            {
                stream?.Close();
            }
        }

        private void HandleChunkedEncoding(TcpClient browser, NetworkStream stream, byte[] buffer, int countHeaderBytes)
        {
            throw new NotImplementedException();
           /* SendResponse(browser, buffer.Take(countHeaderBytes).ToArray());

            var chunkReader = new HttpReader
                (
                  buffer.Skip(countHeaderBytes).ToArray(),
                  countHeaderBytes, NewLine
                );

            byte[] line = chunkReader.ReadLine();
            string chunkSize = Encoding.UTF8.GetString(line);

           /* if (!chunkReader.IsChunkComplete(line))
            {

            }

            int size = Convert.ToInt32(chunkSize);
            int remainingBytes =
                    while (size != 0)
            {
                if (size > readFromStream - countHeaderBytes)
                {
                    byte[] firstBuffer = y
                            var bufferList = new List<byte[]>()
                        }
                byte[] chunk = chunkReader.ReadBytes(chunkSize);

            }

            byte[] x;*/
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