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
            int position = SubArrayIndex(array, subArray);

            return position == -1
                ? position
                : position + subArray.Length;
        }

        public (int first, int second) GetPositions(
             byte[] readLine,
             byte[] firstArray,
             byte[] secondArray)
        {
            (int , int) indexes = (-1, -1);
            int firstCount = 0, secondCount = 0;

            for (int index = 0; index < readLine.Length; index++)
            {
                if (readLine[index] != firstArray[firstCount++])
                {
                    firstCount = 0;
                }

                if (readLine[index] != secondArray[secondCount++])
                {
                    secondCount = 0;
                }

                if (firstCount == firstArray.Length)
                {
                    indexes.Item1 = index - firstCount + 1;
                }

                if (secondCount == secondArray.Length)
                {
                    indexes.Item2 = index - secondCount + 1;
                }
            }

            return indexes;

            //return (GetPosition(readLine, firstArray), GetPosition(readLine, secondArray));
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

        private int SubArrayIndex(byte[] array, byte[] subArray)
        {
            int count = 0;

            for (int index = 0; index < array.Length; index++)
            {
                if (array[index] != subArray[count++])
                {
                    count = 0;
                }

                if (count == subArray.Length)
                {
                    return index - count + 1;
                }
            }

            return -1;
        }
    }
}