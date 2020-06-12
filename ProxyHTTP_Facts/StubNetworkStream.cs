using ProxyServer;
using System;
using System.Linq;
using System.Text;

namespace ProxyHTTP_Facts
{
    internal class StubNetworkStream : INetworkStream
    {
        private readonly byte[] streamBytes;
        private byte[] writtenToStream;
        private int bytesPosition;

        internal StubNetworkStream(string data)
        {
            writtenToStream = null;
            bytesPosition = 0;
            streamBytes = Encoding.UTF8.GetBytes(data);
        }

        internal byte[] GetWrittenBytes => writtenToStream;

        public int Read(byte[] buffer, int offset, int size)
        {
            ThrowReadExceptions(buffer, offset, size);

            int readBytes = 0;
            for (int i = offset; i < size; i++)
            {
                if (i >= streamBytes.Length)
                {
                    return readBytes;
                }

                buffer[i] = streamBytes[bytesPosition++];
                readBytes++;
            }

            return readBytes;
        }

        public void Write(byte[] buffer, int offset, int size)
        {
            if (size == 0)
            {
                return;
            }

            byte[] sessionBytes = new byte[buffer.Length];
            for (int i = offset; i < size; i++)
            {
               sessionBytes[i] = buffer[i];
            }

            writtenToStream = writtenToStream == null
                ? sessionBytes
                : writtenToStream.Concat(sessionBytes).ToArray();
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