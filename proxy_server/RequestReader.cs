using System;
using System.Collections.Generic;
using System.Text;

namespace ProxyHTTP
{
    public class RequestReader
    {
        private string request;

        public RequestReader(string request)
        {
            this.request = request;
        }

        public bool Connect { get; internal set; }

        internal void CheckRequest()
        {
            throw new NotImplementedException();
        }
    }
}
