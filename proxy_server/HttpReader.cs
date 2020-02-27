using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ProxyHTTP
{
    public class HttpReader
    {
        private const string NewLine = "\r\n";
        private readonly byte[] separator;
        private readonly int usedSize;
        private byte[] receivedBytes;

        public HttpReader(byte[] readBytes, int sizeRead, string lineSeparator)
        {
            usedSize = sizeRead;
            receivedBytes = readBytes;
            separator = Encoding.UTF8.GetBytes(lineSeparator);
        }

        public bool IsChunkComplete(byte[] byteLine)
        {
            return byteLine.Skip(byteLine.Length - separator.Length).AsEnumerable()
                   .SequenceEqual(separator);
        }

        public byte[] ReadBytes(string line)
        {
            int chunkSize = int.Parse(line, NumberStyles.HexNumber);

            return receivedBytes.Take(usedSize)
                .Take(chunkSize)
                   .ToArray();
        }

        public byte[] ReadLine()
        {
            var parser = new HttpParser(receivedBytes);
            int index = parser.GetPosition(separator);

            var newBuffer = receivedBytes;
            receivedBytes = receivedBytes.Skip(index).ToArray();

            return index == 2
                ? Encoding.UTF8.GetBytes(NewLine)
                : newBuffer.Take(usedSize)
                    .Take(index - separator.Length)
                      .ToArray();
        }
    }
}