using ProxyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyServer
{
    public class ChunkedEncoding
    {
        private const string Separator = Headers.NewLine;
        private const int BufferSize = 512;

        private readonly INetworkStream browserStream;
        private readonly INetworkStream serverStream;
        private byte[] buffer;
        private ContentLength chunkHandler;
        private HttpParser parser;

        public byte[] Remainder { get; private set; }

        public ChunkedEncoding(INetworkStream serverStream, INetworkStream browserStream)
        {
            //buffer = new byte[BufferSize];
            this.serverStream = serverStream;
            this.browserStream = browserStream;
        }

        internal void ReadAndSendBytes(byte[] bodyPart, int toRead)
        {
            chunkHandler = new ContentLength(serverStream, browserStream);
            chunkHandler.HandleResponseBody(bodyPart, Convert.ToString(toRead));
            Remainder = chunkHandler.Remainder;

        }

        internal int ConvertFromHexadecimal(string hexa)
        {
            return Convert.ToInt32(hexa.Trim(), 16);
        }

        public void HandleChunked(byte[] bodyPart = null)
        {
            /*if (bodyPart != null)
            {
                ReadSizeAndHandleChunk(bodyPart);
                buffer = bodyPart.Concat(buffer).ToArray();
                bodyBytes = bodyPart.Length;

                return;
            }

            int readFromStream = serverStream.Read(buffer, 0, BufferSize);
            ReadSizeAndHandleChunk(buffer.Take(readFromStream).ToArray());*/
        }

        /*


internal void ReadAndSendBytes(int toRead, byte[] bodyPart = null)
{
   InitializeChunkHandler();
   string chunkSize = Convert.ToString(toRead);
   chunkHandler.HandleResponseBody(bodyPart, chunkSize);
}

private int GetChunkSize(byte[] newBuffer)
{
   InitializeParser(newBuffer);
   byte[] readLine = parser.ReadLine(Separator);
   return ConvertFromHexadecimal(Encoding.UTF8.GetString(readLine));
}

private void InitializeChunkHandler()
{
   chunkHandler = new ContentLength(serverStream, browserStream);
}

private void InitializeParser(byte[] bytes)
{
   parser = new HttpParser(bytes);
}

private void ReadSizeAndHandleChunk(byte[] newBuffer)
{
   int chunkSize = GetChunkSize(newBuffer);
   if (chunkSize == 0)
   {
       HandleAfterHeaders();
       return;
   }

   ReadAndSendBytes(chunkSize, parser.GetRemainder().ToArray());
   byte[] remainder = chunkHandler.Remainder;

   if (remainder == null || remainder.Length <= Separator.Length)
   {
       int readFromStream = serverStream.Read(buffer, 0, BufferSize);
       ReadSizeAndHandleChunk(buffer.Take(readFromStream).ToArray());
   }
   else
   {
       ReadSizeAndHandleChunk(remainder.Skip(Separator.Length).ToArray());
   }
}

private void HandleAfterHeaders()
{
   byte[] readLine = parser.ReadLine(Separator);
   if (readLine == null || Encoding.UTF8.GetString(readLine).SequenceEqual(Separator))
   {
       return;
   }

   byte[] toSend = readLine.Concat(
       parser.ReadLine(Headers.EmptyLine))
        .ToArray();

   chunkHandler.HandleResponseBody(toSend, Convert.ToString(toSend.Length));
}
*/
    }
}
