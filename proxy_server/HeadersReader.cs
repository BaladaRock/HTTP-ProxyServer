using System.Linq;

namespace ProxyServer
{
    public class HeadersReader
    {
        private readonly IStreamReader networkStream;
        private byte[] buffer;
        private int position;
        private int bufferSize;
        private int readFromStream;

        public HeadersReader(IStreamReader stream, int size)
        {
            readFromStream = 0;
            networkStream = stream;
            bufferSize = size;
            buffer = new byte[bufferSize];
        }

        public byte[] GetRemainder()
        {
            return buffer.Skip(position).ToArray();
        }

        public void ReadFromStream(int position)
        {
            readFromStream = networkStream.Read(buffer, position, buffer.Length);
        }

        public byte[] ReadHeaders()
        {
            ReadFromStream(0);
            int newBufferSize = bufferSize;

            var parser = new HttpParser(buffer);
            position = parser.GetPosition(buffer, Headers.EmptyLineBytes);
            while (position == -1)
            {
                ReadBytes(newBufferSize);
                newBufferSize += bufferSize;
                position = parser.GetPosition(buffer, Headers.EmptyLineBytes);
            }

            return buffer.Take(position).ToArray();
        }

        private void ReadBytes(int bufferSize)
        {
            buffer = buffer.Concat(new byte[bufferSize]).ToArray();
            ReadFromStream(buffer.Length / 2);
           // buffer = buffer.Take(readFromStream).ToArray();
        }
    }
}