using System;
using System.Net.Sockets;

namespace ProxyServer
{
    public class MyNetworkStream : IStreamReader
    {
        private readonly NetworkStream networkStream;

        public MyNetworkStream(NetworkStream networkStream)
        {
            this.networkStream = networkStream ??
                throw new ArgumentNullException(nameof(networkStream));
        }

        public int Read(byte[] buffer, int offset, int size)
        {
            return networkStream.Read(buffer, offset, size);
        }
    }
}