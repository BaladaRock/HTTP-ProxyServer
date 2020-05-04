using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ProxyHTTP
{
    public class HttpParser
    {
        private byte[] remainingBytes;

        public HttpParser(byte[] buffer)
        {
            remainingBytes = buffer;
        }

        public bool Check(byte[] readLine, byte[] first, byte[] second = null)
        {
            bool checkFirst = Contains(readLine, first);

            if (second != null)
            {
                return checkFirst && Contains(readLine, second);
            }

            return checkFirst;
        }

        public int GetContentLength(byte[] readLine)
        {
            int headerTitle = GetPosition(readLine, Headers.ContentHeader);

            return Convert.ToInt32(Encoding.UTF8.GetString(
                            readLine.Skip(headerTitle)
                                .TakeWhile(bytes => bytes != '\r')
                                    .ToArray()));
        }

        public int GetPosition(byte[] array, byte[] subArray)
        {
            return SubArrayPosition(array, subArray);
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

        internal bool Contains(byte[] array, byte[] subArray)
        {
            return GetPosition(array, subArray) != -1;
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