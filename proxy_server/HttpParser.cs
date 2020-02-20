using System;
using System.Linq;

namespace ProxyHTTP
{
    public class HttpParser
    {
        private readonly byte[] httpResponse;

        public HttpParser()
        {
        }

        public HttpParser(byte[] response)
            : this()
        {
            httpResponse = response;
        }

        public bool Contains(byte[] subArray)
        {
            return GivePosition(subArray, out int position);
        }

        public bool GivePosition(byte[] subArray, out int position)
        {
            int wantedLength = subArray.Length;
            int positionCopy = position = 0;

            var subSets = httpResponse.Select((_, index) =>
                    httpResponse.Skip(index).Take(wantedLength));

            bool checker = subSets.Any(subSet =>
                   {
                       positionCopy = Array.IndexOf(httpResponse, subSet.First());
                       return subSet.SequenceEqual(subArray);
                   });

            position = positionCopy;
            return checker;
        }
    }
}