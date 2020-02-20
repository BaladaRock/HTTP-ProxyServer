using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProxyHTTP;

namespace ProxyHTTP_Facts
{
    public class ChunkReader
    {
        private MemoryStream stream;
        private byte[] buffer;
        private HttpParser lineChecker;

        public ChunkReader(MemoryStream dataStream)
        {
            buffer = new byte[256];
            stream = dataStream;
        }

        public string GetChunkSize()
        {
            stream.Read(buffer);

            lineChecker = new HttpParser();
            return null;
          //  return Encoding.UTF8.GetString(buffer.TakeWhile(x => lineChecker.Contains());
        }

        public byte[] ReadBytes(string line)
        {
            throw new NotImplementedException();
        }
    }
}