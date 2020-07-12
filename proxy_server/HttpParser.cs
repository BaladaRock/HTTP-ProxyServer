using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProxyServer
{
    public class HttpParser
    {
        private readonly byte[] buffer;
        private byte[] remainingBytes;

        public HttpParser(byte[] buffer)
        {
            this.buffer = buffer;
            remainingBytes = buffer;
        }

        public string GetContentLength(byte[] readLine)
        {
            bool check = IsMatch(readLine, Headers.ContentHeader, out string found);
            return check ? found : null;
        }

        public byte[] GetRemainder()
        {
            if (remainingBytes == null)
            {
                return default;
            }

            int remainderLength = remainingBytes.Length;

            return remainderLength == 0 || remainderLength == buffer.Length
                 ? default
                 : remainingBytes;
        }

        public bool IsChunkComplete(byte[] byteLine, string ending, int minimumSize = 0)
        {
            return minimumSize != 0
                ? byteLine.Length >= minimumSize
                : Encoding.UTF8.GetString(byteLine).EndsWith(ending);
        }

        public bool IsChunked(byte[] readLine)
        {
            bool check = IsMatch(readLine, Headers.ChunkedHeader, out string found);
            return check && Regex.Match(found, Headers.Chunked).Success;
        }

        public byte[] ReadLine(string separator)
        {
            if (remainingBytes == null)
            {
                return default;
            }

            int index = GetPosition(remainingBytes, Encoding.UTF8.GetBytes(separator));

            if (index != -1)
            {
                byte[] readLine = remainingBytes.Take(index).ToArray();
                remainingBytes = remainingBytes.Skip(index).ToArray();
                return readLine;
            }

            return remainingBytes;
        }

        internal int GetPosition(byte[] array, byte[] subArray)
        {
            return SubArrayPosition(array, subArray);
        }

        private bool IsMatch(byte[] readLine, string toFind, out string subString)
        {
            string line = Encoding.UTF8.GetString(readLine)
                            .Replace(" ", string.Empty)
                            .Replace("\r\n", string.Empty).ToLower();

            Match match = Regex.Match(line, toFind);
            if (match.Success)
            {
                subString = line.Substring(match.Index + toFind.Length);
                return true;
            }

            subString = null;
            return false;
        }

        private int SubArrayPosition(byte[] array, byte[] subArray, int start = 0)
        {
            int count = 0;

            for (int index = start; index < array.Length; index++)
            {
                if (array[index] != subArray[count++])
                {
                    count = 0;
                }

                if (count == subArray.Length)
                {
                    return index + 1;
                }
            }

            return -1;
        }
    }
}