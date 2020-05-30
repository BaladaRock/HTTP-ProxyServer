using System.Net.Sockets;

namespace ProxyServer
{
    public class ContentLength : INetworkStream
    {
        private NetworkStream stream;
        private byte[] bodyPart;
        private int bodyLength;

        public ContentLength(NetworkStream stream, byte[] bodyPart, int bodyLength)
        {
            this.stream = stream;
            this.bodyPart = bodyPart;
            this.bodyLength = bodyLength;
        }
    }
}