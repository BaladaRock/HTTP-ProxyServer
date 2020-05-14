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

        public HeadersReader(IStreamReader stream, int size)
        {
            readFromStream = 0;
            networkStream = stream;
            buffer = new byte[size];
        }

        public void ReadFromStream(int position)
        {
            readFromStream = networkStream.Read(buffer, position, buffer.Length);
        }

        public byte[] ReadHeaders()
        {
            ReadFromStream(0);

            var parser = new HttpParser(buffer);
            while (parser.GetPosition(buffer, Headers.EmptyLineBytes) == -1)
            {
                buffer = buffer.Concat(new byte[readFromStream]).ToArray();
                ReadFromStream(readFromStream);
                readFromStream += readFromStream;
            }

            return buffer.Take(readFromStream)
                .Take(parser.GetPosition(buffer, Headers.EmptyLineBytes))
                  .ToArray();
        }
    }
}