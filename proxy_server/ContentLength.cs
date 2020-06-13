using System;
using System.Linq;
using System.Net.Sockets;

namespace ProxyServer
{
    public class ContentLength 
    {
        private INetworkStream serverStream;
        private INetworkStream browserStream;
        

        public ContentLength(INetworkStream serverStream, INetworkStream browserStream)
        {
            this.serverStream = serverStream;
            this.browserStream = browserStream;
        }

        internal void HandleResponseBody(byte[] bodyPart, int bodyLength)
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

        internal int ConvertFromHexadecimal(string hexa)
        {
            return Convert.ToInt32(hexa.Trim(), 16);
        }
    }
}