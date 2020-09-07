using System;
using System.Collections.Generic;
using System.Linq;

namespace ProxyServer
{
    public class RequestReader
    {
        private IEnumerable<string> request;

        public RequestReader(string request)
        {
            this.request = request.Trim().Split();
            IsGet = true;
        }

        public int Port { get; private set; }

        public bool IsConnect { get; private set; }

        public bool IsGet { get; private set; }
        public string Host { get; internal set; }

        public void CheckConnect()
        {
            if (request.First() == "CONNECT")
            {
                IsConnect = true;
                IsGet = false;
            }
        }

        internal int GetPort()
        {
            string hostAndPort = string.Concat(request.Skip(1).Take(1).ToArray());
            Port = Convert.ToInt32(hostAndPort.Split(":").Last());
            return Port;
        }
    }
}