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

        private readonly INetworkStream browserStream;
        private readonly INetworkStream serverStream;
        private byte[] buffer;

        public ChunkedEncoding(INetworkStream serverStream, INetworkStream browserStream)
        {
            buffer = new byte[BufferSize];
            this.serverStream = serverStream;
            this.browserStream = browserStream;
        }

        public void HandleChunked(byte[] bodyPart = null)
        {
            int readFromStream = serverStream.Read(buffer, 0, BufferSize);
            bodyPart = buffer.Take(readFromStream).ToArray();

            var parser = new HttpParser(bodyPart);

            byte[] line = parser.ReadLine(Headers.NewLine);
            int chunkSize = ConvertFromHexadecimal(Encoding.UTF8.GetString(line));
            ReadAndSendBytes(Convert.ToString(chunkSize), parser.GetRemainder());
        }

        internal void ReadAndSendBytes(string toRead, byte[] bodyPart = null)
        {
            var bytesReader = new ContentLength(serverStream, browserStream);
            bytesReader.HandleResponseBody(bodyPart, toRead);
        }

        internal int ConvertFromHexadecimal(string hexa)
        {
            return Convert.ToInt32(hexa.Trim(), 16);
        }
    }
}
