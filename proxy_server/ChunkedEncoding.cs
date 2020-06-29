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
        private byte[] buffer;

        public ChunkedEncoding(INetworkStream serverStream, INetworkStream browserStream)
        {
            buffer = new byte[BufferSize];
            this.serverStream = serverStream;
            this.browserStream = browserStream;
        }

        private void InitializeParser(byte[] bodyPart)
        {
            parser = new HttpParser(bodyPart);
        }

        public void HandleChunked(byte[] bodyPart = null)
        {
            int readFromStream = serverStream.Read(buffer, 0, BufferSize);
            bodyPart = buffer.Take(readFromStream).ToArray();
            InitializeParser(bodyPart);

            byte[] line = parser.ReadLine(Separator);
            int chunkSize = ConvertFromHexadecimal(Encoding.UTF8.GetString(line));

            ReadAndSendBytes(chunkSize, parser.GetRemainder());
        }

        internal void ReadAndSendBytes(int toRead, byte[] bodyPart = null)
        {
            string chunkSize = Convert.ToString(toRead);
            var bytesReader = new ContentLength(serverStream, browserStream);

            while (chunkSize != "0")
            {
                bytesReader.HandleResponseBody(bodyPart, chunkSize);
                //bodyPart = bodyPart.Skip(toRead)

            }

        }

        internal int ConvertFromHexadecimal(string hexa)
        {
            return Convert.ToInt32(hexa.Trim(), 16);
        }
    }
}
