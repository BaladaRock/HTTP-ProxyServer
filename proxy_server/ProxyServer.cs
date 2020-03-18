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

            try
            {
                if (stream.CanRead)
                {
                    readFromStream = stream.Read(buffer, 0, buffer.Length);
                }

                var readHeaders = new HttpReader(buffer, readFromStream, Headers.NewLine);

                byte[] readLine = readHeaders.ReadLine();
                int countHeaderBytes = readLine.Length;

                Console.WriteLine(Encoding.UTF8.GetString(readLine));

                while (!readLine.SequenceEqual(Headers.EmptyLineByte))
                {
                    string convertedLine = Encoding.UTF8.GetString(readLine);
                    var lineChecker = new HttpParser(readLine);

                    countHeaderBytes += readLine.Length;

                    if (lineChecker.Contains(Headers.ContentHeader))
                    {
                        HandleContentLength(browser, stream, buffer, readLine, readFromStream);
                        break;
                    }
                    else if (lineChecker.Contains(Headers.ChunkedHeader) && lineChecker.Contains(Headers.Chunked))
                    {
                        HandleChunkedEncoding(browser, stream, buffer, readFromStream);
                        break;
                    }

                    readLine = readHeaders.ReadLine();
                    Console.WriteLine(Encoding.UTF8.GetString(readLine));
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
                SendResponse(browser, buffer.Take(readFromStream - remainingBytes).ToArray());
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