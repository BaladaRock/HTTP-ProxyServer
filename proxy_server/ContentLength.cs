using System;
using System.Linq;
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
            int bodyPartSize = 0;
            if (bodyPart != null)
            {
                bodyPartSize = bodyPart.Length;
                browserStream.Write(bodyPart, 0, bodyPart.Length);
            }

            int read = serverStream.Read(buffer, 0, buffer.Length - bodyPartSize);
            browserStream.Write(buffer.Take(read).ToArray(), 0, read);
        }
    }
}