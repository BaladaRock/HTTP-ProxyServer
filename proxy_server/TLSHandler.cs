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

        public void StartHandshake(int port)
        {
            throw new NotImplementedException();
        }
    }
}