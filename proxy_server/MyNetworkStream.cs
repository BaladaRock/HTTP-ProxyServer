using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ProxyServer
{
    public class MyNetworkStream : INetworkStream
    {
        private readonly NetworkStream networkStream;

        public MyNetworkStream(NetworkStream networkStream)
        {
            this.networkStream = networkStream ??
                throw new ArgumentNullException(nameof(networkStream));
        }

        public int Read(byte[] buffer, int offset, int size)
        {
            WriteMessage(buffer.Skip(offset).Take(size).ToArray());
            return networkStream.Read(buffer, offset, size);
        }

        public void Write(byte[] buffer, int offset, int size)
        {
            WriteMessage(buffer.Skip(offset).Take(size).ToArray());
            networkStream.Write(buffer, offset, size);
        }

        private void WriteMessage(byte[] buffer)
        {
            string message = Encoding.UTF8.GetString(buffer);
            Console.WriteLine($"Proxy has sent response to browser: {message}");
        }
    }
}