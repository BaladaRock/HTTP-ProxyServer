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

        public bool Contains(byte[] subArray)
        {
            return GetPosition(subArray) != -1;
        }

        public int GetPosition(byte[] subArray)
        {
            int position = SubArrayIndex(remainingBytes, subArray);

            return position == -1
                ? position
                : position + subArray.Length;
        }

        public (int first, int second) GetPositions(
             byte[] firstArray,
             byte[] secondArray)
        {
            return (GetPosition(firstArray), GetPosition(secondArray));
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
            int index = GetPosition(endLine);

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
            int? firstIndex = Enumerable.Range(0, array.Length - subArray.Length + 1)
              .Cast<int?>().FirstOrDefault(x => array.Skip((int)x)
                    .Take(subArray.Length).SequenceEqual(subArray));

            return firstIndex != null
                ? (int)firstIndex
                : -1;
        }
    }
}