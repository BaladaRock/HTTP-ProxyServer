﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ProxyServer
{
    public class HeadersReader
    {
        private readonly int bufferSize;
        private readonly INetworkStream networkStream;
        private byte[] buffer;
        private HttpParser parser;
        private IEnumerable<byte> headers;
        private int readFromStream;

        public HeadersReader(INetworkStream stream, int size)
        {
            readFromStream = 0;
            networkStream = stream;
            bufferSize = size;
            buffer = new byte[bufferSize];
            headers = Enumerable.Empty<byte>();
            InitialiseParser();
        }

        private void InitialiseParser()
        {
            ReadFromStream(0);
            parser = new HttpParser(buffer);
        }

        public byte[] GetRemainder()
        {
            return parser.GetRemainder();
        }

        public byte[] ReadHeaders()
        {
            byte[] readLine = parser.ReadLine(Headers.NewLine);

            while (!parser.IsChunkComplete(readLine, Headers.NewLine))
            {
                int oldLength = readFromStream;
                ReadAndResizeBuffer();
                if (readFromStream == oldLength)
                {
                    return default;
                }
                parser = new HttpParser(buffer);
                readLine = parser.ReadLine(Headers.NewLine);
            }

            if (readLine.SequenceEqual(Headers.NewLineByte))
            {
                headers = headers.Concat(readLine);
            }
            else
            {
                headers = readLine;
                ReadHeaders();
            }

            return headers.ToArray();
        }

        private void ReadAndResizeBuffer()
        {
            Array.Resize(ref buffer, buffer.Length + bufferSize);
            ReadFromStream(buffer.Length - bufferSize);
            buffer = buffer.Take(readFromStream).ToArray();
        }

        private void ReadFromStream(int position)
        {
            readFromStream += networkStream.Read(buffer, position, buffer.Length);
        }
    }
}