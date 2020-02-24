using System;
using System.Collections.Generic;
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
            return GetPosition(subArray) != -1;
        }

        public int GetPosition(byte[] subArray)
        {
            int wantedLength = subArray.Length;

            var subSets = httpResponse.Select((_, index) =>
                httpResponse.Skip(index).Take(wantedLength))
                  .FirstOrDefault(x => x.SequenceEqual(subArray));

            return subSets == null
                ? - 1
                : SubArrayIndex(httpResponse, 0, subSets.ToArray()) + wantedLength;
        }

        private int SubArrayIndex(byte[] array, int start, byte[] subArray)
        {
            for (int arrayIndex = start;
                arrayIndex < array.Length - subArray.Length + 1;
                arrayIndex++)
            {
                int count = 0;
                while (count < subArray.Length
                   && subArray[count].Equals(array[arrayIndex + count]))
                {
                    count++;
                }

                if (count == subArray.Length)
                {
                    return arrayIndex;
                }
            }
            return -1;
        }
    }
}