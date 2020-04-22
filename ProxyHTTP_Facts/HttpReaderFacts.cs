using ProxyHTTP;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ProxyHTTP_Facts
{
    public class HttpReaderFacts
    {
        private const string EmptyLine = Headers.EmptyLine;
        private const string NewLine = Headers.NewLine;

        [Fact]
        public void Should_Correctly_ReadLine_For_EmptyLINE()
        {
            // Given
            const string data = "\r\n\r\n";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 4);
            var chunkReader = new HttpReader(buffer);

            // When
            string line = Encoding.UTF8.GetString(chunkReader.ReadLine(EmptyLine));

            // Then
            Assert.Equal(data, line);
        }

        [Fact]
        public void Should_Correctly_ReadLine_More_Complex_Case()
        {
            // Given
            const string data = "23\r\n\r\n\r\n2345";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            byte[] buffer = new byte[12];
            stream.Read(buffer, 0, 12);
            var chunkReader = new HttpReader(buffer);

            // When
            chunkReader.ReadLine(NewLine);
            chunkReader.ReadLine(EmptyLine);
            // Then
            Assert.Equal("2345", Encoding.ASCII.GetString(chunkReader.ReadLine(NewLine)));
        }

        [Fact]
        public void Should_Correctly_ReadLine_Separator_Has_Not_Been_Found()
        {
            // Given
            const string data = "1234";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            var chunkReader = new HttpReader(buffer);

            // When
            string line = Encoding.ASCII.GetString(chunkReader.ReadLine(NewLine));
            // Then
            Assert.Equal("1234", line);
        }

        [Fact]
        public void Should_Read_Bytes_From_ReadLine_Value()
        {
            // Given
            const string data = "3\r\nabc\r\n";
            byte[] toCheck = Encoding.UTF8.GetBytes("abc\r\n");

            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 8);

            var chunkReader = new HttpReader(buffer);

            // When
            string line = Encoding.UTF8.GetString(chunkReader.ReadLine(NewLine));
            byte[] byteLine = chunkReader.ReadBytes(line);

            // Then
            Assert.True(byteLine.SequenceEqual(toCheck));
        }

        [Fact]
        public void Should_Read_Bytes_From_ReadLine_Value_Buffer_Has_More_Elements()
        {
            // Given
            const string data = "3\r\nabc\r\naaaaa";
            byte[] toCheck = Encoding.UTF8.GetBytes("abc\r\n");

            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[24];
            stream.Read(buffer, 0, 24);

            var chunkReader = new HttpReader(buffer);

            // When
            string line = Encoding.UTF8.GetString(chunkReader.ReadLine(NewLine));
            byte[] byteLine = chunkReader.ReadBytes(line);

            // Then
            Assert.True(byteLine.SequenceEqual(toCheck));
        }

        [Fact]
        public void Test_ChunkReader_Should_Read_LeadingLine_For_EmptyLine_Separator()
        {
            // Given
            const string data = "322345\r\n\r\nab";
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[12];

            // When
            stream.Read(buffer, 0, 12);
            var chunkReader = new HttpReader(buffer);
            byte[] line = chunkReader.ReadLine(EmptyLine);

            // Then
            Assert.Equal("322345\r\n\r\n", Encoding.UTF8.GetString(line));
        }

        [Fact]
        public void Test_IS_Chunk_Compete_Should_Work_For_Given_Minimum_Chunk_Size()
        {
            // Given
            const string data = "3\r\nabc\r\n2b";
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[10];
            stream.Read(buffer, 0, 10);
            var chunkReader = new HttpReader(buffer);

            // When
            string line = Encoding.UTF8.GetString(chunkReader.ReadLine(NewLine));
            byte[] byteLine = chunkReader.ReadBytes(line);
            int size = Convert.ToInt32(line);
            // Then
            Assert.True(chunkReader.IsChunkComplete(byteLine, NewLine, size));
        }

        [Fact]
        public void Test_IS_Chunk_Compete_Should_Work_For_Simple_Case()
        {
            // Given
            const string data = "12345";
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[5];
            stream.Read(buffer, 0, 5);
            var chunkReader = new HttpReader(buffer);

            // When
            byte[] byteLine = chunkReader.ReadBytes("5\r\n");

            // When, Then
            Assert.False(chunkReader.IsChunkComplete(byteLine, NewLine));
        }

        [Fact]
        public void Test_ReadBytes_Should_Work_Correctly_for_MULTIPLE_Reads()
        {
            // Given
            const string data = "2b2\r\n\r\n\r\n3\r\n222\r\n";
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[32];

            // When
            stream.Read(buffer, 0, 32);
            var chunkReader = new HttpReader(buffer);
            chunkReader.ReadLine(NewLine);
            chunkReader.ReadLine(EmptyLine);
            string line = Encoding.UTF8.GetString(chunkReader.ReadLine(NewLine));
            string byteLine = Encoding.UTF8.GetString(chunkReader.ReadBytes(line));

            // Then
            Assert.Equal("222\r\n", byteLine);
        }
    }
}