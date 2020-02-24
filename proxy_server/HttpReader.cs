using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProxyHTTP;
using System.Globalization;

namespace ProxyHTTP
{
    public class HttpReader
    {
        private readonly byte[] receivedBytes;
        private readonly byte[] separator;
        private int index;

        public HttpReader(byte[] readBytes, string lineSeparator)
        {
            receivedBytes = readBytes;
            separator = Encoding.UTF8.GetBytes(lineSeparator);
        }

        public byte[] ReadLine()
        {
            var parser = new HttpParser(receivedBytes);
            index = parser.GetPosition(separator);

            return receivedBytes.Take(index - separator.Length)
                .ToArray();
        }

        public byte[] ReadBytes(string line)
        {
            int chunkSize = int.Parse(line, NumberStyles.HexNumber);

            return receivedBytes.Skip(index).Take(chunkSize)
                .ToArray();
        }

        public bool IsChunkComplete(byte[] byteLine)
        {
            return byteLine.Skip(byteLine.Length - separator.Length).AsEnumerable()
                   .SequenceEqual(separator);
        }
    }
}