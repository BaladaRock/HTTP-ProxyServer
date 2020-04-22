using ProxyHTTP;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                    HandleResponse(client, stream);
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
            return request?.StartsWith("GET ") == true;
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

        private void HandleResponse(TcpClient browser, NetworkStream stream)
        {
            byte[] buffer = new byte[512];
            int readFromStream = 0;
            int countHeaderBytes = 0;
            bool isChunked = false;

            try
            {
                if (stream.CanRead)
                {
                    readFromStream = stream.Read(buffer, 0, buffer.Length);
                }

                var readHeaders = new HttpReader(buffer);

                byte[] readLine = readHeaders.ReadLine(Headers.NewLine);
                Console.Write(Encoding.UTF8.GetString(readLine));

                while (!readLine.SequenceEqual(Headers.NewLineByte))
                {
                    countHeaderBytes += readLine.Length;
                    var lineChecker = new HttpParser(readLine);

                    if (lineChecker.Contains(Headers.ContentHeader))
                    {
                        HandleContentLength(browser, stream, buffer, readLine, readFromStream);
                        break;
                    }
                    else if (lineChecker.Contains(Headers.ChunkedHeader) && lineChecker.Contains(Headers.Chunked))
                    {
                        isChunked = true;
                    }

                    readLine = readHeaders.ReadLine(Headers.NewLine);
                    Console.Write(Encoding.UTF8.GetString(readLine));
                }

                if (isChunked)
                {
                    int realbytesLength = countHeaderBytes + readLine.Length;
                    SendResponse(browser, buffer.Take(realbytesLength).ToArray());
                    HandleChunkedEncoding(
                        browser,
                        stream,
                        buffer.Skip(realbytesLength).Take(readFromStream - realbytesLength)
                          .ToArray()
                          );
                }

                browser.Close();
            }
            catch (Exception exception)
            {
                stream?.Close();
            }
        }

        private void HandleContentLength(TcpClient browser, NetworkStream stream, byte[] buffer, byte[] contentLine, int readFromStream)
        {
            int headerTitle = new HttpParser(contentLine).GetPosition(Headers.ContentHeader);

            int bodyLength = Convert.ToInt32(Encoding.UTF8.GetString(
                            contentLine.Skip(headerTitle)
                                .TakeWhile(bytes => bytes != '\r')
                                    .ToArray()));

            int headerBytes = new HttpParser(buffer.Take(readFromStream).ToArray())
                .GetPosition(Headers.EmptyLineByte);
            int remainingBytes = readFromStream - headerBytes;

            if (remainingBytes > bodyLength)
            {
                SendResponse(browser, buffer.Take(headerBytes + remainingBytes).ToArray());
            }
            else
            {
                SendResponse(browser, buffer.Take(readFromStream).ToArray());
                remainingBytes = bodyLength - remainingBytes;

                while (remainingBytes > 512)
                {
                    readFromStream = stream.Read(buffer, 0, buffer.Length);
                    remainingBytes -= readFromStream;
                    SendResponse(browser, buffer.Take(readFromStream).ToArray());
                }

                readFromStream = stream.Read(buffer, 0, remainingBytes);
                SendResponse(browser, buffer.Take(readFromStream).ToArray());
            }
        }

        private void HandleChunkedEncoding(TcpClient browser, NetworkStream stream, byte[] bytes)
        {
            var chunkReader = new HttpReader(bytes);
            byte[] readLine = chunkReader.ReadLine(Headers.NewLine);
            string chunkLine = Encoding.UTF8.GetString(readLine);

            while (chunkLine != Headers.ZeroChunk)
            {
                while (!chunkReader.IsChunkComplete(readLine, Headers.NewLine, 3))
                {
                    var newBytes = new byte[512];
                    int toRead = stream.Read(newBytes, 0, 512);
                    bytes = bytes.Concat(newBytes.Take(toRead)).ToArray();
                    chunkReader = new HttpReader(bytes);
                    readLine = chunkReader.ReadLine(Headers.NewLine);
                }

                int lineLength = readLine.Length;
                int chunkSize = Convert.ToInt32(
                             chunkLine.Substring(0, lineLength - 2),
                             16);

                SendResponse(browser, readLine);

                SendChunkToBrowser(stream, browser, chunkSize, bytes.Skip(lineLength).ToArray());

                bytes = new byte[512];
                int readFromStream = stream.Read(bytes, 0, 512);
                chunkReader = new HttpReader(bytes.Take(readFromStream).ToArray());
                readLine = chunkReader.ReadLine(Headers.NewLine);
                chunkLine = Encoding.UTF8.GetString(readLine);
            }

            SendResponse(browser, chunkReader.ReadLine(Headers.NewLine));
        }

        private void SendChunkToBrowser(NetworkStream stream, TcpClient browser, int chunkSize, byte[] bytes)
        {
            foreach (var buffer in MakeBufferList(stream, chunkSize, bytes))
            {
                SendResponse(browser, buffer);
            }
        }

        private List<byte[]> MakeBufferList(NetworkStream stream, int chunkSize, byte[] bytes)
        {
            int readFromStream = 0;
            var listElement = new byte[512];
            int listSize = (chunkSize / 512) + 1;
            List<byte[]> bufferList = new List<byte[]>(listSize)
            {
                bytes
            };

            if (chunkSize <= bytes.Length)
            {
                return bufferList;
            }

            int remainingBytes = chunkSize - bytes.Length;
            while (remainingBytes > 512)
            {
                readFromStream = stream.Read(listElement, 0, 512);
                bufferList.Add(listElement.Take(readFromStream).ToArray());
                remainingBytes -= 512;
            }

            readFromStream = stream.Read(listElement, 0, remainingBytes + 2);
            bufferList.Add(listElement.Take(readFromStream).ToArray());

            return bufferList;
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