using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ProxyHTTP
{
    public class HttpReader
    {
        private byte[] remainingBytes;

        public HttpReader(byte[] readBytes)
        {
            remainingBytes = readBytes;
        }

        public bool IsChunkComplete(byte[] byteLine, string ending, int size = 0)
        {
            return size != 0
                ? byteLine.Length == size + ending.Length
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
            byte[] readLine = null;

            var parser = new HttpParser(remainingBytes);
            int index = parser.GetPosition(endLine);

            if (index != -1)
            {
                readLine = remainingBytes.Take(index).ToArray();
                remainingBytes = remainingBytes.Skip(index).ToArray();
            }

            return readLine ?? remainingBytes;
        }
    }
}