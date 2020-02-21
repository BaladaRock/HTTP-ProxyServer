using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProxyHTTP;
using System.Globalization;

namespace ProxyHTTP_Facts
{
    public class HttpReader
    {
        private byte[] buffer;

        public HttpReader(byte[] readBytes)
        {
            buffer = readBytes;
        }

        public byte[] ReadLine()
        {
            return null;
            /*stream.Read(buffer);

            int position = lineChecker.GivePosition(endlineMarker);
            return Encoding.UTF8.GetString(buffer.Take(position).ToArray());*/
        }

        public byte[] ReadBytes(string line)
        {
            return null;
           /* int bytesToRead = int.Parse(line, NumberStyles.HexNumber);

            int position = lineChecker.GivePosition(endlineMarker);
            return buffer.Skip(position + 2).Take(bytesToRead).ToArray();*/
        }
    }
}