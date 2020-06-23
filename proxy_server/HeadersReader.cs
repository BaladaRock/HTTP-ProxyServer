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
        private IEnumerable<byte> headers;
        private HttpParser parser;
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

        public byte[] Remainder { get; private set; }

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
                buffer = parser.GetRemainder() ?? GetNewBuffer();

                parser = new HttpParser(buffer);
                readFromStream -= readLine.Length;
                ReadHeaders();
            }

            var byteHeaders = headers.ToArray();

            Remainder = parser.GetRemainder();
            return parser.IsChunkComplete(byteHeaders, Headers.EmptyLine)
                ? byteHeaders
                : default;
        }

        private byte[] GetNewBuffer()
        {
            ReadAndResizeBuffer();
            return buffer.Skip(buffer.Length - bufferSize).ToArray();
        }

        private bool CheckReadProcess()
        {
            int oldLength = readFromStream;
            ReadAndResizeBuffer();

            return readFromStream == oldLength;
        }

        private byte[] GetHeader(byte[] readLine)
        {
            while (!parser.IsChunkComplete(readLine, NewLine))
            {
                if (CheckReadProcess())
                {
                    return default;
                }

                parser = new HttpParser(buffer);
                readLine = parser.ReadLine(NewLine);
            }

            return readLine;
        }

        private void InitialiseParser()
        {
            ReadFromStream(0);
            parser = new HttpParser(buffer);
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