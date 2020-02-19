using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ProxyHTTP
{
    public class HttpParser
    {
        private byte[] httpResponse;

        public HttpParser(byte[] response)
        {
            httpResponse = response;
        }

        public bool Contains(byte[] subArray)
        {
            int wantedLength = subArray.Length;

            var subSets = httpResponse.Select((_, index) =>
                    httpResponse.Skip(index).Take(wantedLength))
                        .Where(newArray => newArray.Count() == wantedLength);

            return subSets.Any(subSet => subSet.SequenceEqual(subArray));
        }
    }
}
