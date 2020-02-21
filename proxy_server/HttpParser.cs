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
            return GetPosition(subArray) != subArray.Length - 1;
        }

        public int GetPosition(byte[] subArray)
        {
            int wantedLength = subArray.Length;

            var subSets = httpResponse.Select((_, index) =>
                httpResponse.Skip(index).Take(wantedLength))
                  .ToArray();

            return Array.IndexOf(
                 httpResponse,
                 Array.Find(subSets, subSet =>
                     subSet.SequenceEqual(subArray))?.First()) + wantedLength;
        }
    }
}