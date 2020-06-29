using System;
using System.Linq;

namespace ProxyServer
{
    public class ContentLength
    {
        private const int BufferSize = 512;

        private readonly INetworkStream browserStream;
        private readonly INetworkStream serverStream;

        public ContentLength(INetworkStream serverStream, INetworkStream browserStream)
        {
            this.serverStream = serverStream;
            this.browserStream = browserStream;
        }

        public void HandleResponseBody(byte[] bodyPart, string bytesToRead)
        {
            int remainingBytes = Convert.ToInt32(bytesToRead.Trim());
            if (bodyPart != null)
            {
                if (bodyPart.Length >= remainingBytes)
                {
                    WriteOnStream(bodyPart, remainingBytes);
                    return;
                }

                browserStream.Write(bodyPart, 0, bodyPart.Length);
                remainingBytes -= bodyPart.Length;
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
            WriteOnStream(buffer, readFromStream);
        }

        private void WriteOnStream(byte[] buffer, int toWrite)
        {
            browserStream.Write(buffer.Take(toWrite).ToArray(), 0, toWrite);
        }
    }
}