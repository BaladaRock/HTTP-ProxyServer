using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace ProxyServer
{
    public class HeadersReader
    {
        private readonly IStreamReader networkStream;
        private byte[] buffer;
        private int readFromStream;

        public HeadersReader(IStreamReader stream)
        {
            this.networkStream = stream;
            readFromStream = 0;
            buffer = new byte[512];
        }

        public void ReadFromStream()
        {
            readFromStream = networkStream.Read(buffer, 0, buffer.Length);
        }

        public byte[] ReadHeaders()
        {
            ReadFromStream();

            var parser = new HttpParser(buffer);
            return buffer.Take(readFromStream)
                .Take(parser.GetPosition(buffer, Headers.EmptyLineBytes))
                  .ToArray();
        }
    }
}