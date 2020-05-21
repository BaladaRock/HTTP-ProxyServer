using System;
using System.Linq;

namespace ProxyServer
{
    public class HeadersReader
    {
        private readonly int bufferSize;
        private readonly IStreamReader networkStream;
        private byte[] buffer;
        private int position;
        private int readFromStream;

        public HeadersReader(IStreamReader stream, int size)
        {
            readFromStream = 0;
            networkStream = stream;
            bufferSize = size;
            buffer = new byte[bufferSize];
        }

        public byte[] GetRemainder()
        {
            return buffer.Skip(position).ToArray();
        }

        public void ReadFromStream(int position)
        {
            readFromStream = networkStream.Read(buffer, position, buffer.Length);
        }

        public byte[] ReadHeaders()
        {
            ReadFromStream(0);

            var parser = new HttpParser(buffer);
            position = parser.GetPosition(buffer, Headers.EmptyLineBytes);
            while (position == -1)
            {
                ReadAndResizeBuffer();
                position = parser.GetPosition(buffer, Headers.EmptyLineBytes);
            }

            return buffer.Take(position).ToArray();
        }

        private void ReadAndResizeBuffer()
        {
            Array.Resize(ref buffer, buffer.Length + bufferSize);
            ReadFromStream(buffer.Length - bufferSize);
        }
    }
}