using System;
using System.Collections.Generic;
using System.Linq;

namespace ProxyServer
{
    public class HeadersReader
    {
        private const string NewLine = Headers.NewLine;
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

        private bool CheckReadProcess(int readBytes)
        {
            int oldLength = readBytes;
            ReadAndResizeBuffer();

            return readFromStream == oldLength;
        }

        private byte[] GetHeader(byte[] readLine)
        {
            while (!parser.IsChunkComplete(readLine, NewLine))
            {
                if (CheckReadProcess(readFromStream))
                {
                    return default;
                }

                parser = new HttpParser(buffer);
                readLine = parser.ReadLine(NewLine);
            }

            return readLine;
        }

        public byte[] ReadHeaders()
        {
            byte[] readLine = GetHeader(parser.ReadLine(NewLine));
            if (readLine == null)
            {
                return default;
            }

            headers = headers.Concat(readLine);

            if (!readLine.SequenceEqual(Headers.NewLineByte))
            {
                byte[] remainder = parser.GetRemainder();
                if (remainder == null)
                {
                    int oldLength = readFromStream;
                    if (CheckReadProcess(oldLength))
                    {
                        return default;
                    }

                    buffer = buffer.Skip(oldLength).ToArray();
                }
                else
                {
                    buffer = remainder;
                }

                parser = new HttpParser(buffer);
                ReadHeaders();
            }

            var byteHeaders = headers.ToArray();
            return parser.IsChunkComplete(byteHeaders, Headers.EmptyLine)
                ? byteHeaders
                : null;
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