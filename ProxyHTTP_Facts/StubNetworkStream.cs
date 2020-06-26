using ProxyServer;
using System;
using System.Linq;
using System.Text;

namespace ProxyHTTP_Facts
{
    internal class StubNetworkStream : INetworkStream
    {
        private readonly byte[] streamBytes;
        private int bytesPosition;

        internal StubNetworkStream(string data)
        {
            GetWrittenBytes = null;
            bytesPosition = 0;
            streamBytes = Encoding.UTF8.GetBytes(data);
        }

        internal byte[] GetWrittenBytes { get; private set; }

        internal byte[] GetReadBytes { get; private set; }

        public int Read(byte[] buffer, int offset, int size)
        {
            ThrowReadWriteExceptions(buffer, offset, size);

            int readBytes = 0;
            for (int i = offset; i < size; i++)
            {
                if (i >= streamBytes.Length || bytesPosition >= streamBytes.Length)
                {
                    //GetReadBytes = buffer.Skip(offset).Take(readBytes).ToArray();
                    return readBytes;
                }

                buffer[i] = streamBytes[bytesPosition++];
                readBytes++;
            }

            GetReadBytes = buffer.Skip(offset).Take(size).ToArray();
            return readBytes;
        }

        public void Write(byte[] buffer, int offset, int size)
        {
            if (size == 0)
            {
                return;
            }

            ThrowReadWriteExceptions(buffer, offset, size);

            byte[] sessionBytes = new byte[buffer.Length];
            for (int i = offset; i < size; i++)
            {
               sessionBytes[i] = buffer[i];
            }

            GetWrittenBytes = GetWrittenBytes == null
                ? sessionBytes
                : GetWrittenBytes.Concat(sessionBytes).ToArray();
        }

        private static void ThrowReadWriteExceptions(byte[] buffer, int offset, int size)
        {
            if (size - offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(offset));
            }
        }
    }
}