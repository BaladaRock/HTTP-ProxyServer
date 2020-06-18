using System;
using System.Collections.Generic;
using System.Linq;

namespace ProxyServer
{
    public class HeadersReader
    {
        private readonly int bufferSize;
        private readonly INetworkStream networkStream;
        private byte[] buffer;
        private IEnumerable<byte> headers;
        private int separatorPosition;
        private int readFromStream;

        public HeadersReader(INetworkStream stream, int size)
        {
            readFromStream = 0;
            networkStream = stream;
            bufferSize = size;
            buffer = new byte[bufferSize];
            headers = Enumerable.Empty<byte>();
        }

        public byte[] GetRemainder()
        {
            return separatorPosition == 0 || separatorPosition == buffer.Length
                ? (default)
                : buffer.Skip(separatorPosition).ToArray();
        }

        public byte[] ReadHeaders()
        {
            ReadFromStream(0);

            var parser = new HttpParser(buffer);
            byte[] readLine = parser.ReadLine(Headers.NewLine);

            while (!readLine.SequenceEqual(Headers.NewLineByte))
            {
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

                headers = readLine;
                readLine = parser.ReadLine(Headers.NewLine);
            }

            headers = headers.Concat(readLine);
            separatorPosition = headers.ToArray().Length;

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