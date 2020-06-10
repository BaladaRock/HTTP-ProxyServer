using ProxyServer;
using System;
using System.Text;

namespace ProxyHTTP_Facts
{
    internal class StubNetworkStream : INetworkStream
    {
        private readonly byte[] bytes;
        private int bytesPosition;

        internal StubNetworkStream(string data)
        {
            Data = data;
            bytesPosition = 0;
            bytes = Encoding.UTF8.GetBytes(Data);
        }

        internal string Data { get; set; }

        internal byte[] GetWrittenBytes { get; set; }

        public int Read(byte[] buffer, int offset, int size)
        {
            ThrowReadExceptions(buffer, offset, size);

            int readBytes = 0;
            for (int i = offset; i < size; i++)
            {
                if (i >= bytes.Length)
                {
                    return readBytes;
                }

                buffer[i] = bytes[bytesPosition++];
                readBytes++;
            }

            return readBytes;
        }

        public void Write(byte[] buffer, int offset, int size)
        {
            throw new NotImplementedException();
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