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

        public bool Contains(byte[] array, byte[] subArray)
        {
            return GetPosition(array, subArray) != -1;
        }

        public string GetContentLength(byte[] readLine)
        {
            string line = Encoding.UTF8.GetString(readLine).ToLower();
            line = line.Replace(" ", string.Empty)
                       .Replace("\r\n", string.Empty);

            Match match = Regex.Match(line, Headers.ContentHeader);

            if (match.Success)
            {
                return line.Substring(line.IndexOf(':') + 1);
            }

            return null;
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

        public bool IsChunked(byte[] readLine)
        {
            string line = Encoding.UTF8.GetString(readLine).ToLower();
            line = line.Replace(" ", string.Empty)
                       .Replace("\r\n", string.Empty);

            Match match = Regex.Match(line, Headers.ChunkedHeader);

            return match.Success &&
                   Regex.Match(line.Substring(match.Index), Headers.Chunked).Success;
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