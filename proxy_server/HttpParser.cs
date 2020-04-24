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
            return GetPosition(subArray) != -1;
        }

        public int GetPosition(byte[] subArray)
        {
            int position = SubArrayIndex(httpResponse, 0, subArray);

            return position == -1
                ? position
                : position + subArray.Length;
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