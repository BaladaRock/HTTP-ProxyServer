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
            networkStream = stream;
            bufferSize = size;

            InitializeFields();
        }

        public bool Chunked { get; private set; }

        public int ContentLength { get; private set; }

        public byte[] Remainder { get; private set; }

        public byte[] ReadHeaders(bool checker = false)
        {
            byte[] readLine = GetHeader(parser.ReadLine(NewLine));

            if (readLine == null)
            {
                return default;
            }

            CheckHeader(checker, readLine);
            headers = headers.Concat(readLine);

            if (!readLine.SequenceEqual(Headers.NewLineByte))
            {
                buffer = parser.GetRemainder() ?? GetNewBuffer();
                parser = new HttpParser(buffer);
                readFromStream -= readLine.Length;
                ReadHeaders(true);
            }

            Remainder = parser.GetRemainder();
            var byteHeaders = headers.ToArray();

            return parser.IsChunkComplete(byteHeaders, Headers.EmptyLine)
                ? byteHeaders
                : default;
        }

        private void CheckForHeaderFields(byte[] readLine)
        {
            if (Chunked || ContentLength >= 0)
            {
                return;
            }

            if (parser.IsChunked(readLine))
            {
                Chunked = true;
            }
            else if (uint.TryParse(parser.GetContentLength(readLine), out uint value))
            {
                ContentLength = (int)value;
            }
        }

        private void CheckHeader(bool checker, byte[] readLine)
        {
            if (!checker)
            {
                return;
            }

            CheckForHeaderFields(readLine);
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

        private byte[] GetNewBuffer()
        {
            ReadAndResizeBuffer();
            return buffer.Skip(buffer.Length - bufferSize).ToArray();
        }

        private void InitializeFields()
        {
            ContentLength = -1;
            headers = Enumerable.Empty<byte>();
            buffer = new byte[bufferSize];

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