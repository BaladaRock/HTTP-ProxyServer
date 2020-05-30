using System;
using System.Linq;

namespace ProxyServer
{
    public class HeadersReader
    {
        private readonly int bufferSize;
        private readonly INetworkStream networkStream;
        private byte[] buffer;
        private int position;
        private int readFromStream;

        public HeadersReader(INetworkStream stream, int size)
        {
            readFromStream = 0;
            networkStream = stream;
            bufferSize = size;
            buffer = new byte[bufferSize];
        }

        public byte[] GetRemainder()
        {
            return readFromStream == 0 || position == buffer.Length - 1
                ? (default)
                : buffer.Skip(position).ToArray();
        }

        public byte[] ReadHeaders()
        {
            ReadFromStream(0);

            var parser = new HttpParser(buffer);
            while (GetEmptyLinePosition(parser) == -1)
            {
                if (readFromStream == 0)
                {
                    return default;
                }

                ReadAndResizeBuffer();
            }

            return buffer.Take(position).ToArray();
        }

        private void ReadAndResizeBuffer()
        {
            Array.Resize(ref buffer, buffer.Length + bufferSize);
            ReadFromStream(buffer.Length - bufferSize);
        }

        private void ReadFromStream(int position)
        {
            readFromStream = networkStream.Read(buffer, position, buffer.Length);
        }

        private int GetEmptyLinePosition(HttpParser parser)
        {
            position = parser.GetPosition(buffer, Headers.EmptyLineBytes);
            return position;
        }
    }
}