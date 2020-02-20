using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProxyHTTP;
using System.Globalization;

namespace ProxyHTTP_Facts
{
    public class ChunkReader
    {
        private Stream stream;
        private byte[] buffer;
        private HttpParser lineChecker;
        private byte[] endlineMarker;

        public ChunkReader(Stream dataStream)
        {
            endlineMarker = Encoding.ASCII.GetBytes("\r\n");

            buffer = new byte[256];
            lineChecker = new HttpParser(buffer);
            stream = dataStream;
        }

        public string ReadLine()
        {
            stream.Read(buffer);

            int position = lineChecker.GivePosition(endlineMarker);
            return Encoding.UTF8.GetString(buffer.Take(position).ToArray());
        }

        public byte[] ReadBytes(string line)
        {
            int bytesToRead = int.Parse(line, NumberStyles.HexNumber);

            int position = lineChecker.GivePosition(endlineMarker);
            return buffer.Skip(position + 2).Take(bytesToRead).ToArray();
        }
    }
}