using System;
using System.Net.Sockets;

namespace ProxyServer
{
    public class ContentLength 
    {
        private INetworkStream serverStream;
        private INetworkStream browserStream;
        private byte[] bodyPart;
        private int bodyLength;

        public ContentLength(INetworkStream serverStream, INetworkStream browserStream, byte[] bodyPart, int bodyLength)
        {
            this.serverStream = serverStream;
            this.browserStream = browserStream;
            this.bodyPart = bodyPart;
            this.bodyLength = bodyLength;
        }

        internal void HandleResponseBody()
        {
            byte[] buffer = new byte[bodyLength];
            int read = serverStream.Read(buffer, 0, buffer.Length);
            browserStream.Write(buffer, 0, read);
        }
    }
}