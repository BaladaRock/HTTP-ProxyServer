using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProxyServer
{
    public class RequestReader
    {
        private IEnumerable<string> request;

        public RequestReader(string request)
        {
            this.request = Regex.Split(request.Trim(), "\r\n");
            IsGet = true;
        }

        public int Port { get; private set; }

        public bool IsConnect { get; private set; }

        public bool IsGet { get; private set; }

        public string Host
        {
            get
            {
                string hostHeader = request.FirstOrDefault(
                    x => x.StartsWith("Host:"));

                return hostHeader == null
                    ? null
                    : string.Concat(string.Concat(hostHeader.Split()).Skip(5));
            }
        }

        public void CheckConnect()
        {
            if (request.First().StartsWith("CONNECT"))
            {
                IsConnect = true;
                IsGet = false;
            }
        }

        internal int GetPort()
        {
            Port = Convert.ToInt32(request.First()
                     .Split(':').Last()
                       .Split().First());
            return Port;
        }
    }
}