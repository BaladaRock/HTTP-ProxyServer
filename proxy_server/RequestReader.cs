using System;
using System.Collections.Generic;
using System.Text;

namespace ProxyServer
{
    public class RequestReader
    {
        private string request;

        public RequestReader(string request)
        {
            this.request = request;
        }

        public bool Connect { get; private set; }

        public void CheckRequest()
        {
            if (string.Concat(request.Split()).StartsWith("CONNECT"))
            {
                Connect = true;
            }
        }
    }
}
