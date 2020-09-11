using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProxyServer
{
    public class RequestReader
    {
        private readonly IEnumerable<string> compressedRequest;
        private string hostHeader;

        public RequestReader(string request)
        {
            compressedRequest = Regex.Split(request.Trim(), "\r\n");
            Request = request;
            IsGet = true;
            SetHost();
        }

        public string Host { get; private set; }

        public bool IsConnect { get; private set; }

        public bool IsGet { get; private set; }

        public string Request { get; }

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
            if (compressedRequest.First().StartsWith("CONNECT"))
            {
                IsConnect = true;
                IsGet = false;
            }
        }

        private void SetHost()
        {
            hostHeader = compressedRequest.FirstOrDefault(
                  x => x.StartsWith("Host:"));

            Host = hostHeader?.Split(':')[1].Trim();
        }
    }
}