using ProxyServer;
using System;
using System.Linq;
using System.Text;

namespace ProxyServer_Facts
{
    internal class StubNetworkStream : IStreamReader
    {
        private readonly byte[] bytes;
        private int count;

        internal StubNetworkStream(string data)
        {
            Data = data;
            count = 0;
            bytes = Encoding.UTF8.GetBytes(Data);
        }

        internal string Data { get; set; }

        public int Read(byte[] buffer, int offset, int size)
        {
            ThrowReadExceptions(buffer, offset, size);

            for (int i = offset; i < size; i++)
            {
                buffer[i] = bytes[count++];
            }

            return count;
        }

        private static void ThrowReadExceptions(byte[] buffer, int offset, int size)
        {
            if (size - offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(offset));
            }
        }
    }
}