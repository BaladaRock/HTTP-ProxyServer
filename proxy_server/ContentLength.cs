using System;
using System.Net.Sockets;

namespace ProxyServer
{
    public class ContentLength 
    {
        private INetworkStream stream;
        private byte[] bodyPart;
        private int bodyLength;

        public ContentLength(INetworkStream stream, byte[] bodyPart, int bodyLength)
        {
            this.stream = stream;
            this.bodyPart = bodyPart;
            this.bodyLength = bodyLength;
        }

        internal void HandleResponseBody()
        {
            throw new NotImplementedException();
        }
    }
}