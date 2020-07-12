using System;
using System.Linq;
using System.Text;

namespace ProxyServer
{
    public class ChunkedEncoding
    {
        private const int BufferSize = 512;
        private const string Separator = Headers.NewLine;
        private readonly INetworkStream browserStream;
        private readonly INetworkStream serverStream;
        private byte[] buffer;
        private ContentLength chunkHandler;
        private HttpParser parser;

        public ChunkedEncoding(INetworkStream serverStream, INetworkStream browserStream)
        {
            buffer = new byte[BufferSize];
            this.serverStream = serverStream;
            this.browserStream = browserStream;
        }

        public byte[] Remainder { get; private set; }

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

        internal int ConvertFromHexadecimal(string hexa)
        {
            return Convert.ToInt32(hexa.Trim(), 16);
        }

        internal void ReadAndSendChunk(byte[] bodyPart, int toRead)
        {
            chunkHandler = new ContentLength(serverStream, browserStream);
            chunkHandler.HandleResponseBody(bodyPart, Convert.ToString(toRead));
            Remainder = chunkHandler.Remainder;
        }

        private bool CheckAndReadBytes(byte[] line, int minimumSize)
        {
            if (IsLineComplete(line, minimumSize))
            {
                return true;
            }

            int readFromStream = serverStream.Read(buffer, 0, BufferSize);
            buffer = line.Concat(buffer.Take(readFromStream)).ToArray();

            return false;
        }

        private byte[] GetNewBuffer()
        {
            int readFromStream = serverStream.Read(buffer, 0, BufferSize);
            return buffer.Take(readFromStream)
                .SkipWhile(x => Headers.NewLineByte.Contains(x))
                  .ToArray();
        }

        private void HandleAfterHeaders()
        {
            byte[] readLine = parser.ReadLine(Separator);
            if (!CheckAndReadBytes(readLine, 0))
            {
                InitializeParser(buffer);
                HandleAfterHeaders();
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

        private void InitializeParser(byte[] bytes)
        {
            parser = new HttpParser(bytes);
        }

        private bool IsLineComplete(byte[] line, int minimumSize)
        {
            return parser.IsChunkComplete(line, Separator, minimumSize);
        }

        private void ReadAndSendBytes(byte[] bodyPart)
        {
            InitializeParser(bodyPart);
            byte[] readSize = parser.ReadLine(Separator);
            if (!CheckAndReadBytes(readSize, 3))
            {
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
    }
}