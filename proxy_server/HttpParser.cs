using System;
using System.Linq;

namespace ProxyHTTP
{
    public class HttpParser
    {
        private readonly byte[] httpResponse;

        public HttpParser(byte[] response)
        {
            httpResponse = response;
        }

        public bool Contains(byte[] subArray)
        {
            return GivePosition(subArray) != -1;
        }

        public int GivePosition(byte[] subArray)
        {
            int wantedLength = subArray.Length;

            var subSets = httpResponse.Select((_, index) =>
                httpResponse.Skip(index).Take(wantedLength))
                  .ToArray();

            return Array.IndexOf(
                 httpResponse,
                 Array.Find(subSets, subSet =>
                     subSet.SequenceEqual(subArray))?.First());
        }
    }
}