using System;
using System.Linq;

namespace ProxyServer
{
    public class ContentLength
    {
        private const int BufferSize = 512;

        private readonly INetworkStream browserStream;
        private readonly INetworkStream serverStream;
        private byte[] buffer;
        private int readFromStream;

        public ContentLength(INetworkStream serverStream, INetworkStream browserStream)
        {
            this.serverStream = serverStream;
            this.browserStream = browserStream;
            buffer = new byte[BufferSize];
            readFromStream = 0;
        }

        public byte[] Remainder { get; internal set; }

        public void HandleResponseBody(byte[] bodyPart, string bytesToRead)
        {
            int toRead = Convert.ToInt32(bytesToRead.Trim());

            if (bodyPart != null)
            {
                if (bodyPart.Length >= toRead)
                {
                    WriteOnStream(bodyPart, toRead);
                    SetRemainder(bodyPart, toRead);
                    return;
                }

                WriteOnStream(bodyPart, bodyPart.Length);
                toRead -= bodyPart.Length;
            }

            HandleRemainingBody(toRead);
        }

        private void SetRemainder(byte[] buffer, int toRead)
        {
            if (toRead == buffer.Length)
            {
                return;
            }

            Remainder = buffer.Skip(toRead).ToArray();
        }

        private void CheckAndResize(ref byte[] buffer, int length)
        {
            if (length == buffer.Length)
            {
                return;
            }

            buffer = buffer.Take(length).ToArray();
        }

        private void HandleRemainingBody(int toRead)
        {
            while (toRead > BufferSize)
            {
                ReadAndWrite(BufferSize);
                toRead -= BufferSize;
            }

            ReadAndWrite(toRead);
        }

        private void ReadAndWrite(int size)
        {
            readFromStream = serverStream.Read(buffer, 0, size);
            WriteOnStream(buffer, readFromStream);
        }

        private void WriteOnStream(byte[] buffer, int toWrite)
        {
            CheckAndResize(ref buffer, toWrite);
            browserStream.Write(buffer, 0, toWrite);
        }
    }
}