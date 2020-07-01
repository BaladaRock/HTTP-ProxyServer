using ProxyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyHTTP
{
    public class ChunkedEncoding
    {
        private const int BufferSize = 512;
        private const string Separator = Headers.NewLine;

        private readonly INetworkStream browserStream;
        private readonly INetworkStream serverStream;
        private HttpParser parser;
        private ContentLength chunkHandler;
        private byte[] buffer;

        public ChunkedEncoding(INetworkStream serverStream, INetworkStream browserStream)
        {
            buffer = new byte[BufferSize];
            this.serverStream = serverStream;
            this.browserStream = browserStream;
        }

        private void InitializeParser(byte[] bytes)
        {
            parser = new HttpParser(bytes);
        }

        public void HandleChunked(byte[] bodyPart = null)
        {
            int readFromStream = serverStream.Read(buffer, 0, BufferSize);
            InitializeParser(buffer.Take(readFromStream).ToArray());

            byte[] readLine = parser.ReadLine(Separator);
            int chunkSize = ConvertFromHexadecimal(Encoding.UTF8.GetString(readLine));

            while (chunkSize != 0)
            {
                ReadAndSendBytes(
                    chunkSize,
                    parser.GetRemainder().ToArray());
                byte[] remainder = chunkHandler.Remainder;
                if (remainder == null)
                {
                    readFromStream = serverStream.Read(buffer, 0, BufferSize);
                    InitializeParser(buffer.Take(readFromStream).ToArray());
                    readLine = parser.ReadLine(Separator);
                    chunkSize = ConvertFromHexadecimal(Encoding.UTF8.GetString(readLine));

                }
                else
                {
                    InitializeParser(remainder.Skip(Separator.Length).ToArray());
                    readLine = parser.ReadLine(Separator);
                    chunkSize = ConvertFromHexadecimal(Encoding.UTF8.GetString(readLine));
                }
            }
            //ReadAndSendBytes(chunkSize, parser.GetRemainder());
        }

        internal void ReadAndSendBytes(int toRead, byte[] bodyPart = null)
        {
            InitializeChunkHandler();
            string chunkSize = Convert.ToString(toRead);
            chunkHandler.HandleResponseBody(bodyPart, chunkSize);
        }

        internal int ConvertFromHexadecimal(string hexa)
        {
            return Convert.ToInt32(hexa.Trim(), 16);
        }

        private void InitializeChunkHandler()
        {
            chunkHandler = new ContentLength(serverStream, browserStream);
        }
    }
}
