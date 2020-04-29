using System.Globalization;
using System.Linq;
using System.Text;

namespace ProxyHTTP
{
    public class HttpParser
    {
        private byte[] remainingBytes;

        public HttpParser(byte[] readBytes)
        {
            remainingBytes = readBytes;
        }

        public bool Contains(byte[] array, byte[] subArray)
        {
            return GetPosition(array, subArray) != -1;
        }

        public int GetPosition(byte[] array, byte[] subArray)
        {
            return SubArrayPosition(array, subArray);
        }

        public (int first, int second) GetPositions(
             byte[] readLine,
             byte[] firstArray,
             byte[] secondArray)
        {
            (int first, int second) indexes = SubArraysPositions(
                readLine,
                firstArray,
                secondArray,
                out int count);

            if (indexes.first != -1)
            {
                indexes.second = SubArrayPosition(readLine, secondArray, indexes.first, count);
            }
            else if (indexes.second != -1)
            {
                indexes.first = SubArrayPosition(readLine, firstArray, indexes.second, count);
            }

            return indexes;
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

        private int SubArrayPosition(byte[] array, byte[] subArray, int start = 0, int countIndex = 0)
        {
            int count = countIndex;

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

        private (int first, int second) SubArraysPositions(
             byte[] array,
             byte[] firstArray,
             byte[] secondArray,
             out int countStart,
             int positionStart = 0)
        {
            (int, int) count = default;

            for (int index = positionStart; index < array.Length; index++)
            {
                if (array[index] != firstArray[count.Item1++])
                {
                    count.Item1 = 0;
                }

                if (array[index] != secondArray[count.Item2++])
                {
                    count.Item2 = 0;
                }

                if (count.Item1 == firstArray.Length)
                {
                    countStart = count.Item2;
                    return (index + 1, -1);
                }

                if (count.Item2 == secondArray.Length)
                {
                    countStart = count.Item1;
                    return (-1, index + 1);
                }
            }

            countStart = 0;
            return (-1, -1);
        }
    }
}