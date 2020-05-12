using ProxyServer;
using System;
using System.Text;

namespace ProxyServer_Facts
{
    internal class StubNetworkStream : IStreamReader
    {
        private byte[] bytes;

        internal StubNetworkStream(string data)
        {
            Data = data;
            bytes = Encoding.UTF8.GetBytes(Data);
        }

        internal string Data { get; set; }

        public int Read(byte[] buffer, int offset, int size)
        {
            if (size - offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(offset));
            }

            int count = 0;

            for(int i = offset; i < size; i++)
            {
                buffer[i] = bytes[count++];

                if (count == bytes.Length)
                {
                    return count;
                }
            }

            return count;
        }
    }
}