using ProxyHTTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ProxyHTTP_Facts
{
    public class HttpReaderFacts
    {
        [Fact]
        public void Test_ChunkReader_Should_Read_LeadingLine_for_given_CHUNK()
        {
            // Given
            const string data = "322345\r\nabc";
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[512];

            // When
            stream.Read(buffer, 0, 512);
            var chunkReader = new HttpReader(buffer);
            byte[] line = chunkReader.ReadLine();

            // Then
            Assert.Equal("322345", Encoding.UTF8.GetString(line));
        }

        [Fact]
        public void Test_LineReader_Should_Read_Bytes_for_given_chunk_SIZE()
        {
            // Given
            const string data = "3\r\nabc\r\n2b";
            byte[] toCheck = Encoding.UTF8.GetBytes("abc");

            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[512];
            stream.Read(buffer, 0, 512);

            var chunkReader = new HttpReader(buffer);

            // When
            string line = Encoding.UTF8.GetString(chunkReader.ReadLine());
            byte[] byteLine = chunkReader.ReadBytes(line);

            // Then
            Assert.True(byteLine.SequenceEqual(toCheck));
        }

        /*[Fact]
        public void Test_LineReader_Should_Work_Correctly_For_Multiple_Reads()
        {
            // Given
            const string data = "3\r\nabc\r\n2ba\r\n4\abcd\r\n";
            byte[] toCheck = Encoding.UTF8.GetBytes("abcbaabcd");

            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            var chunkReader = new HttpReader(stream);

            // When
            string line = null;
            byte[] byteLine = new byte[256];

            line = chunkReader.ReadLine();
            byteLine.Concat(chunkReader.ReadBytes(line));
            line = chunkReader.ReadLine();
            byteLine.Concat(chunkReader.ReadBytes(line));
            line = chunkReader.ReadLine();
            byteLine.Concat(chunkReader.ReadBytes(line));

            // Then
            Assert.True(byteLine.SequenceEqual(toCheck));
        }*/
    }
}
