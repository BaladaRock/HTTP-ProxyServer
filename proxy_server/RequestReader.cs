using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProxyServer
{
    public class RequestReader
    {
        private readonly IEnumerable<string> request;

        public RequestReader(string request)
        {
            this.request = Regex.Split(request.Trim(), "\r\n");
            IsGet = true;
        }

        public string Host
        {
            get
            {
                string hostHeader = request.FirstOrDefault(
                    x => x.StartsWith("Host:"));

                return hostHeader?.Split().Last();
            }
        }

        public bool IsConnect { get; private set; }

        public bool IsGet { get; private set; }

        public int Port
        {
            get
            {
                return int.TryParse(request.First().Split(':')
                               .Last().Trim().Split().First(),
                       out int port) ? port : 0;
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
    }
}