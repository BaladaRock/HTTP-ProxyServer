using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProxyServer
{
    public class RequestReader
    {
        private readonly IEnumerable<string> request;
        private string hostHeader;

        public RequestReader(string request)
        {
            this.request = Regex.Split(request.Trim(), "\r\n");
            IsGet = true;
            SetHost();
        }

        public string Host { get; private set; }

        public bool IsConnect { get; private set; }

        public bool IsGet { get; private set; }

        public int Port
        {
            get
            {
                return hostHeader != null && int.TryParse(hostHeader.Split(':').Last(),
                       out int port) ? port : 0;
            }
        }

        public void CheckRequestType()
        {
            if (request.First().StartsWith("CONNECT"))
            {
                IsConnect = true;
                IsGet = false;
            }
        }

        private void SetHost()
        {
            hostHeader = request.FirstOrDefault(
                  x => x.StartsWith("Host:"));

            Host = hostHeader?.Split(':')[1].Trim();
        }
    }
}