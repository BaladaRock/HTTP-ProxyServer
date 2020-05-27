using System;
using System.Linq;

namespace ProxyServer
{
    public class HeadersReader
    {
        private readonly IStreamReader networkStream;
        private byte[] buffer;

        public HeadersReader(IStreamReader stream, int size)
        {
            ReadFromStream = 0;
            networkStream = stream;
            BufferSize = size;
            buffer = new byte[BufferSize];
        }

        private int BufferSize { get; }

        private int Position { get; set; }

        private int ReadFromStream { get; set; }

        public byte[] GetRemainder()
        {
            return ReadFromStream == 0 || Position == buffer.Length - 1
                ? (default)
                : buffer.Skip(Position).ToArray();
        }

        public byte[] ReadHeaders()
        {
            ReadBytes(0);

            var parser = new HttpParser(buffer);
            while (GetEmptyLinePosition(parser) == -1)
            {
                if (ReadFromStream == 0)
                {
                    return default;
                }

                ReadAndResizeBuffer();
            }

            return buffer.Take(Position).ToArray();
        }

        private int GetEmptyLinePosition(HttpParser parser)
        {
            Position = parser.GetPosition(buffer, Headers.EmptyLineBytes);
            return Position;
        }

        private void ReadAndResizeBuffer()
        {
            Array.Resize(ref buffer, buffer.Length + BufferSize);
            ReadBytes(buffer.Length - BufferSize);
        }

        private void ReadBytes(int position)
        {
            ReadFromStream = networkStream.Read(buffer, position, buffer.Length);
        }
    }
}