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

        private int size;

        public HeadersReader(IStreamReader stream, int size)
        {
            networkStream = stream;
            this.size = size;
            readFromStream = 0;
            buffer = new byte[size];
        }

        public void ReadFromStream()
        {
            readFromStream = networkStream.Read(buffer, 0, size);
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