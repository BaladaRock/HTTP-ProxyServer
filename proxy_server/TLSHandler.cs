using System;
using System.Net.Sockets;

namespace ProxyServer
{
    public class TLSHandler
    {
        private TcpClient browser;

        private TcpClient httpsServer;

        public TLSHandler(TcpClient browser)
        {
            this.browser = browser;
        }

        public void StartHandshake(string host, int port)
        {
            throw new NotImplementedException();
        }
    }
}