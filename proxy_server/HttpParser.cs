using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProxyServer
{
    public class HttpParser
    {
        private byte[] remainingBytes;
        private readonly byte[] buffer;

        public HttpParser(byte[] buffer)
        {
            this.buffer = buffer;
            remainingBytes = buffer;
        }

        public bool IsChunkComplete(byte[] byteLine, string ending, int minimumSize = 0)
        {
            return minimumSize != 0
                ? byteLine.Length >= minimumSize + ending.Length
                : Encoding.UTF8.GetString(byteLine).EndsWith(ending);
        }

        public byte[] ReadBytes(string line)
        {
            int chunkSize = int.Parse(line, NumberStyles.HexNumber);
            return remainingBytes.Take(chunkSize + Headers.NewLine.Length).ToArray();
        }

        public byte[] ReadLine(string separator)
        {
            if (remainingBytes == null)
            {
                return default;
            }

            byte[] endLine = Encoding.UTF8.GetBytes(separator);
            int index = GetPosition(remainingBytes, endLine);

            if (index != -1)
            {
                byte[] readLine = remainingBytes.Take(index).ToArray();
                remainingBytes = remainingBytes.Skip(index).ToArray();
                return readLine;
            }

            return remainingBytes;
        }

        public string GetContentLength(byte[] readLine)
        {
            bool check = IsMatch(readLine, Headers.ContentHeader, out string found);

            return check ? found : null;
        }

        public bool IsChunked(byte[] readLine)
        {
            bool check = IsMatch(readLine, Headers.ChunkedHeader, out string found);

            return check && Regex.Match(found, Headers.Chunked).Success;
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

        internal int GetPosition(byte[] array, byte[] subArray)
        {
            return SubArrayPosition(array, subArray);
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
    }
}