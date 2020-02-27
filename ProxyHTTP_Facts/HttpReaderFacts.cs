using ProxyHTTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ProxyHTTP_Facts
{
    [System.Runtime.InteropServices.Guid("2E155147-F8BB-4525-B716-1F5CF5263B12")]
    public class HttpReaderFacts
    {
        private const string LineSeparator = "\r\n";

        [Fact]
        public void Test_ChunkReader_Should_Read_LeadingLine_for_given_CHUNK()
        {
            // Given
            const string data = "322345\r\nabc";
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[11];

            // When
            stream.Read(buffer, 0, 11);
            var chunkReader = new HttpReader(buffer, 11, LineSeparator);
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
            byte[] buffer = new byte[32];
            stream.Read(buffer, 0, 32);

            var chunkReader = new HttpReader(buffer, 32, LineSeparator);

            // When
            string line = Encoding.UTF8.GetString(chunkReader.ReadLine());
            byte[] byteLine = chunkReader.ReadBytes(line);

            // Then
            Assert.True(byteLine.SequenceEqual(toCheck));
        }

        [Fact]
        public void Should_Return_RemainingBytes_When_Chunk_Is_NOT_Complete()
        {
            // Given
            const string data = "3\r\nab";
            byte[] toCheck = Encoding.UTF8.GetBytes("ab");

            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[5];
            stream.Read(buffer, 0, 5);

            var chunkReader = new HttpReader(buffer, 5, LineSeparator);

            // When
            string line = Encoding.UTF8.GetString(chunkReader.ReadLine());
            byte[] byteLine = chunkReader.ReadBytes(line);

            // Then
            Assert.True(byteLine.SequenceEqual(toCheck));
            Assert.False(chunkReader.IsChunkComplete(byteLine));
        }

        [Fact]
        public void Should_Know_That_Buffer_Is_Not_Used_Entirely()
        {
            // Given
            const string data = "3\r\nabc";

            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 5);

            var chunkReader = new HttpReader(buffer, 5, LineSeparator);

            // When
            string line = Encoding.UTF8.GetString(chunkReader.ReadLine());
            byte[] byteLine = chunkReader.ReadBytes(line);

            // Then
            Assert.True(byteLine.SequenceEqual(Encoding.UTF8.GetBytes("ab\0")));
            Assert.False(chunkReader.IsChunkComplete(byteLine));
        }

        [Fact]
        public void Test_LineReader_Should_Do_Multiple_Reads()
        {
            // Given
            const string data = "2b2\r\nabc\r\n3\r\n222\r\n";
            byte[] toCheck = Encoding.UTF8.GetBytes("222");

            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[32];
            stream.Read(buffer, 0, 32);

            var chunkReader = new HttpReader(buffer, 32, LineSeparator);

            // When
            chunkReader.ReadLine();
            chunkReader.ReadLine();
            string line = Encoding.UTF8.GetString(chunkReader.ReadLine());
            byte[] byteLine = chunkReader.ReadBytes(line);
            string test = Encoding.UTF8.GetString(byteLine);

            // Then
            Assert.True(byteLine.SequenceEqual(toCheck));
        }
    }
}
