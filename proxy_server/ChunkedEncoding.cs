using ProxyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyServer
{
    public class ChunkedEncoding
    {
        private const string Separator = Headers.NewLine;
        private const int BufferSize = 512;

        private readonly INetworkStream browserStream;
        private readonly INetworkStream serverStream;
        private byte[] buffer;
        private ContentLength chunkHandler;
        private HttpParser parser;

        public byte[] Remainder { get; private set; }

        public ChunkedEncoding(INetworkStream serverStream, INetworkStream browserStream)
        {
            buffer = new byte[BufferSize];
            this.serverStream = serverStream;
            this.browserStream = browserStream;
        }

        internal void ReadAndSendChunk(byte[] bodyPart, int toRead)
        {
            chunkHandler = new ContentLength(serverStream, browserStream);
            chunkHandler.HandleResponseBody(bodyPart, Convert.ToString(toRead));
            Remainder = chunkHandler.Remainder;
        }

        internal int ConvertFromHexadecimal(string hexa)
        {
            return Convert.ToInt32(hexa.Trim(), 16);
        }

        private void InitializeParser(byte[] bytes)
        {
            parser = new HttpParser(bytes);
        }

        public void HandleChunked(byte[] bodyPart = null)
        {
            if (bodyPart == null)
            {
                int readFromStream = serverStream.Read(buffer, 0, BufferSize);
                ReadAndSendBytes(buffer.Take(readFromStream).ToArray());
            }
            else
            {
                ReadAndSendBytes(bodyPart);
            }
        }

        private void ReadAndSendBytes(byte[] bodyPart)
        {
            InitializeParser(bodyPart);
            byte[] readSize = parser.ReadLine(Separator);
            if (!IsLineComplete(readSize))
            {
                int readFromStream = serverStream.Read(buffer, 0, BufferSize);
                buffer = readSize.Concat(buffer.Take(readFromStream)).ToArray();
                ReadAndSendBytes(buffer);
                return;
            }

            int chunkSize = ConvertFromHexadecimal(Encoding.UTF8.GetString(readSize));
            if (chunkSize == 0)
            {
                HandleAfterHeaders();
                return;
            }

            ReadAndSendChunk(parser.GetRemainder(), chunkSize);
            byte[] remainder = chunkHandler.Remainder;

            if (remainder == null || remainder.Length <= Separator.Length)
            {
                ReadAndSendBytes(GetNewBuffer());
            }
            else
            {
                ReadAndSendBytes(remainder.Skip(Separator.Length).ToArray());
            }
        }

        private byte[] GetNewBuffer()
        {
            int readFromStream = serverStream.Read(buffer, 0, BufferSize);
            return buffer.Take(readFromStream)
                .SkipWhile(x => Headers.NewLineByte.Contains(x))
                  .ToArray();
        }

        private bool IsLineComplete(byte[] readSize)
        {
            return parser.IsChunkComplete(readSize, Separator, 3);
        }

        private void HandleAfterHeaders()
        {
            byte[] readLine = parser.ReadLine(Separator);
            if (!parser.IsChunkComplete(readLine, Separator))
            {
                int readFromStream = serverStream.Read(buffer, 0, BufferSize);
                buffer = readLine.Concat(buffer.Take(readFromStream)).ToArray();
                readLine = parser.ReadLine(Separator);
            }

            if (readLine == null || Encoding.UTF8.GetString(readLine) == Separator)
            {
                return;
            }

            byte[] toSend = readLine.Concat(
                parser.ReadLine(Headers.EmptyLine))
                 .ToArray();

            chunkHandler.HandleResponseBody(toSend, Convert.ToString(toSend.Length));
        }
    }
}
