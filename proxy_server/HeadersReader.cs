using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace ProxyServer
{
    public class HeadersReader
    {
        private readonly NetworkStream stream;
        private byte[] buffer;
        private int readFromStream;

        public HeadersReader(IStreamReader stream)
        {
            this.stream = (NetworkStream)stream;
            readFromStream = 0;
            buffer = new byte[512];
        }

        public void ReadFromStream()
        {
            readFromStream = stream.Read(buffer, 0, buffer.Length);
        }

        public byte[] ReadHeaders()
        {
            ReadFromStream();

            return buffer.Take(readFromStream).ToArray();
        }
    }
}