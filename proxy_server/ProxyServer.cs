﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("ProxyHTTP_Facts")]

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

                TcpClient browser = proxy.AcceptTcpClient();
                NetworkStream browserStream = browser.GetStream();
                Console.WriteLine("Connected!");

                string request = GetRequest(browserStream, browser);

                if (CheckRequest(request))
                {
                    try
                    {
                        string host = GetHost(request);
                        NetworkStream hostStream = new TcpClient(host, 80).GetStream();
                        SendRequest(hostStream, request);
                        HandleResponse(browser, hostStream);
                    }
                    catch (Exception hostException)
                    {
                        //hostName does not exist
                    }
                }

                browser.Close();
            }
        }

        private bool CheckRequest(string request)
        {
            return request?.StartsWith("GET ") == true;
        }

        private string GetHost(string request)
        {
            string[] requestDetails = request.Trim().Split('\n');
            string hostHeader = requestDetails[1];
            int lastPosition = hostHeader.LastIndexOf('\r');

            return hostHeader.Substring(6, lastPosition - 6);
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

        private string GetString(byte[] readLine)
        {
            return Encoding.UTF8.GetString(readLine);
        }

        private void HandleChunked(TcpClient browser, NetworkStream stream, byte[] bytes)
        {
            var read = ReadAndSendBytes(
                browser,
                stream,
                bytes,
                new HttpParser(bytes),
                Headers.EmptyLine);

            var parser = new HttpParser(read.remainder);
            byte[] readLine = parser.ReadLine(Headers.NewLine);
            string chunkLine = GetString(readLine);

            while (chunkLine != Headers.ZeroChunk)
            {
                SendChunkToBrowser(
                   stream,
                   browser,
                   Convert.ToInt32(chunkLine.Substring(0, chunkLine.Length - 2), 16),
                   read.remainder.Skip(readLine.Length).ToArray());

                bytes = new byte[512];
                int readFromStream = stream.Read(bytes, 0, 512);
                parser = new HttpParser(bytes.Take(readFromStream).ToArray());
                readLine = parser.ReadLine(Headers.NewLine);
                chunkLine = Encoding.UTF8.GetString(readLine);
            }

            /* read = ReadAndSendBytes(
                browser,
                stream,
                read.remainder,
                new HttpParser(read.remainder),
                Headers.NewLine);

             string chunkLine = GetString(read.readLine);
             while (chunkLine != Headers.ZeroChunk)
             {
                 SendChunkToBrowser(
                     stream,
                     browser,
                     Convert.ToInt32(chunkLine.Substring(0, chunkLine.Length - 2), 16),
                     read.remainder);

                 bytes = new byte[512];
                 int readFromStream = stream.Read(bytes, 0, bytes.Length);

                 read = ReadAndSendBytes(
                 browser,
                 stream,
                 bytes.Take(readFromStream).ToArray(),
                 new HttpParser(bytes.Take(readFromStream).ToArray()),
                 Headers.NewLine);

                 chunkLine = GetString(read.readLine);
             }*/

            /* var chunkReader = new HttpParser(bytes);
        byte[] readLine = chunkReader.ReadLine(Headers.NewLine);
        string chunkLine = Encoding.UTF8.GetString(readLine);

        while (chunkLine != Headers.ZeroChunk)
        {
            while (!chunkReader.IsChunkComplete(readLine, Headers.NewLine, 3))
            {
                var newBytes = new byte[512];
                int toRead = stream.Read(newBytes, 0, 512);
                bytes = bytes.Concat(newBytes.Take(toRead)).ToArray();
                chunkReader = new HttpParser(bytes);
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
            chunkReader = new HttpParser(bytes.Take(readFromStream).ToArray());
            readLine = chunkReader.ReadLine(Headers.NewLine);
            chunkLine = Encoding.UTF8.GetString(readLine);
        }

        var endingChunk = chunkReader.ReadLine(Headers.NewLine);
        SendResponse(browser, endingChunk);
        if (endingChunk.Length > 2)
        {
            SendResponse(browser, chunkReader.ReadLine(Headers.EmptyLine));
        }*/
        }

        private void HandleContentLength(
            NetworkStream serverStream,
            NetworkStream browserStream,
            byte[] bodyPart,
            int bodyLength)
        {
            var contentHandler = new ContentLength(
                new MyNetworkStream(serverStream),
                new MyNetworkStream(browserStream));

            contentHandler.HandleResponseBody(bodyPart, Convert.ToString(bodyLength));
        }

        private void HandleResponse(TcpClient browser, NetworkStream serverStream)
        {
            try
            {
                NetworkStream browserStream = browser.GetStream();

                var handleHeaders = new HeadersReader(new MyNetworkStream(serverStream), 512);
                byte[] headers = handleHeaders.ReadHeaders();
                byte[] remainder = handleHeaders.Remainder;
                SendResponse(browser, headers);

                int contentPosition = handleHeaders.ContentLength;
                if (contentPosition != -1)
                {
                    HandleContentLength(
                        serverStream,
                        browserStream,
                        remainder,
                        handleHeaders.ContentLength);
                }
                else if (handleHeaders.Chunked)
                {
                    HandleChunked(browser, serverStream, remainder);
                }

               // browser.Close();
            }
            catch (Exception exception)
            {
                serverStream?.Close();
            }
        }

        private List<byte[]> MakeBufferList(NetworkStream stream, int chunkSize, byte[] bytes)
        {
            int readFromStream = 0, bufferLength = 512;
            var listElement = new byte[bufferLength];
            int listSize = (chunkSize / bufferLength) + 1;
            List<byte[]> bufferList = new List<byte[]>(listSize);

            if (chunkSize <= bufferLength)
            {
                bufferList.Add(bytes.Take(chunkSize).ToArray());
                return bufferList;
            }
            else
            {
                bufferList.Add(bytes);
            }

            int remainingBytes = chunkSize - bytes.Length;
            while (remainingBytes > bufferLength)
            {
                readFromStream = stream.Read(listElement, 0, bufferLength);
                bufferList.Add(listElement.Take(readFromStream).ToArray());
                remainingBytes -= bufferLength;
            }

            readFromStream = stream.Read(listElement, 0, remainingBytes);
            bufferList.Add(listElement.Take(readFromStream).ToArray());

            return bufferList;
        }

        private (byte[] remainder, byte[] readLine) ReadAndSendBytes(
            TcpClient browser,
            NetworkStream stream,
            byte[] buffer,
            HttpParser parser,
            string separator)
        {
            byte[] readLine = parser.ReadLine(separator);

            while (readLine.Length == buffer.Length)
            {
                SendResponse(browser, readLine);
                int readFromStream = stream.Read(buffer, 0, buffer.Length);
                parser = new HttpParser(buffer.Take(readFromStream).ToArray());
                readLine = parser.ReadLine(separator);
            }

            SendResponse(browser, readLine);
            return (buffer.Skip(readLine.Length).ToArray(), readLine);
        }

        private void SendChunkToBrowser(NetworkStream stream, TcpClient browser, int chunkSize, byte[] bytes)
        {
            foreach (var buffer in MakeBufferList(stream, chunkSize, bytes))
            {
                SendResponse(browser, buffer);
            }
        }

        private void SendRequest(NetworkStream stream, string request)
        {
            stream.Write(Encoding.UTF8.GetBytes(request));
            Console.WriteLine($"Proxy has sent request to host: {request}");
        }

        private void SendResponse(TcpClient browser, byte[] response)
        {
            browser.Client.Send(response);

            string textResponse = Encoding.UTF8.GetString(response);
            Console.WriteLine($"Proxy has sent host response back to browser: {textResponse}");
        }
    }
}