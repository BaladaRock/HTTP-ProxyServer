using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

[assembly: InternalsVisibleTo("ProxyHTTP_Facts")]

namespace ProxyServer
{
    public class ProxyServer
    {
        private readonly string proxyIP;

        public ProxyServer(string proxyIP)
        {
            this.proxyIP = proxyIP;
        }

        public void StartProxy()
        {
            TcpListener proxy = new TcpListener(IPAddress.Parse(proxyIP), 8080);
            proxy.Start();

            while (true)
            {
                Console.Write("Waiting for web client... ");

                TcpClient browser = proxy.AcceptTcpClient();
                Console.WriteLine("Connected!");

                var sessionHandler = new SessionHandler(browser);
                Thread session = new Thread(() => sessionHandler.StartSession());
                session.Start();
            }
        }
    }
}