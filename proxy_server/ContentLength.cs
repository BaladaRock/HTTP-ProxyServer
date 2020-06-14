﻿using System;
using System.Linq;

namespace ProxyServer
{
    internal class ContentLength
    {
        private const int BufferSize = 512;

        private readonly INetworkStream browserStream;
        private readonly INetworkStream serverStream;

        internal ContentLength(INetworkStream serverStream, INetworkStream browserStream)
        {
            this.serverStream = serverStream;
            this.browserStream = browserStream;
        }

        internal void HandleResponseBody(byte[] bodyPart, string bytesToRead)
        {
            int remainingBytes = Convert.ToInt32(bytesToRead.Trim());
            if (bodyPart != null)
            {
                remainingBytes -= bodyPart.Length;
                browserStream.Write(bodyPart, 0, bodyPart.Length);
            }

            HandleRemainingBody(remainingBytes);
        }

        private void HandleRemainingBody(int remainingBytes)
        {
            byte[] buffer = new byte[BufferSize];
            int readFromStream = 0;

            while (remainingBytes > BufferSize)
            {
                readFromStream = serverStream.Read(buffer, 0, BufferSize);
                browserStream.Write(buffer, 0, readFromStream);
                remainingBytes -= BufferSize;
            }

            readFromStream = serverStream.Read(buffer, 0, remainingBytes);
            browserStream.Write(buffer.Take(readFromStream).ToArray(), 0, readFromStream);
        }
    }
}