using ProxyServer;
using System.IO;
using System.Text;
using Xunit;

namespace ProxyHTTP_Facts
{
    public class HttpParserFacts
    {
        private const string EmptyLine = Headers.EmptyLine;
        private const string NewLine = Headers.NewLine;

        [Fact]
        public void Test_GetPosition_Array_Has_More_SubArrays()
        {
            // Given
            const string data = "bcbcababab\r\n";
            const string subArray = "ab";

            // When
            var parser = new HttpParser(Encoding.UTF8.GetBytes(data));
            byte[] readLine = parser.ReadLine(NewLine);
            int position = parser.GetPosition(readLine, Encoding.UTF8.GetBytes(subArray));

            // Then
            Assert.Equal(6, position);
        }

        [Fact]
        public void Test_GetPosition_for_EMPTY_LINE()
        {
            // Given
            const string data = "abcd\r\n\r\nab";
            const string subArray = "\r\n\r\n";

            // When
            var parser = new HttpParser(Encoding.UTF8.GetBytes(data));
            byte[] readLine = parser.ReadLine("A");
            int position = parser.GetPosition(readLine, Encoding.UTF8.GetBytes(subArray));

            // Then
            Assert.Equal(8, position);
        }

        [Fact]
        public void Test_GetPosition_Should_Take_the_ENTIRE_SUBARRAY()
        {
            // Given
            const string data = "abefabcd";
            const string subArray = "abcd";

            // When
            var parser = new HttpParser(Encoding.UTF8.GetBytes(data));
            byte[] readLine = parser.ReadLine(EmptyLine);
            int position = parser.GetPosition(readLine, Encoding.UTF8.GetBytes(subArray));

            // Then
            Assert.Equal(8, position);
        }

        [Fact]
        public void Test_GetPosition_SubArray_Is_At_Start()
        {
            // Given
            const string data = "abbcababab";
            const string subArray = "ab";

            // When
            var parser = new HttpParser(Encoding.UTF8.GetBytes(data));
            byte[] readLine = parser.ReadLine(EmptyLine);
            int position = parser.GetPosition(readLine, Encoding.UTF8.GetBytes(subArray));

            //Then
            Assert.Equal(2, position);
        }

        [Fact]
        public void Test_GetPosition_SubArray_Is_At_The_End()
        {
            // Given
            const string data = "abcd";
            const string subArray = "cd";

            // When
            var parser = new HttpParser(Encoding.UTF8.GetBytes(data));
            byte[] readLine = parser.ReadLine(NewLine);

            // Then
            Assert.Equal(4, parser.GetPosition(readLine, Encoding.UTF8.GetBytes(subArray)));
        }

        [Fact]
        public void Test_GetRemainder_After_Multiple_Reads()
        {
            // Given
            const string data = "1234\r\n\r\nabc";
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            var parser = new HttpParser(buffer);

            // When
            parser.ReadLine(NewLine);
            parser.ReadLine(NewLine);
            byte[] remainder = parser.GetRemainder();

            // Then
            Assert.Equal("abc", Encoding.UTF8.GetString(remainder));
        }

        [Fact]
        public void Test_GetRemainder_After_Simple_Read()
        {
            // Given
            const string data = "1234\r\n\r\nabc";
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            var parser = new HttpParser(buffer);

            // When
            parser.ReadLine(EmptyLine);
            byte[] remainder = parser.GetRemainder();

            // Then
            Assert.Equal("abc", Encoding.UTF8.GetString(remainder));
        }

        [Fact]
        public void Test_GetRemainder_Should_Return_NULL_When_EntireBuffer_WasRead()
        {
            // Given
            const string data = "1234abc";
            const string separator = "c";
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            var parser = new HttpParser(buffer);

            // When
            parser.ReadLine(separator);
            byte[] remainder = parser.GetRemainder();

            // Then
            Assert.Null(remainder);
        }

        [Fact]
        public void Test_GetRemainder_ShouldReturn_NULL_When_NO_Separator_Has_been_found()
        {
            // Given
            const string data = "1234abc";
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            var parser = new HttpParser(buffer);

            // When
            parser.ReadLine(NewLine);
            byte[] remainder = parser.GetRemainder();

            // Then
            Assert.Null(remainder);
        }

        [Fact]
        public void Test_GetRemainder_When_Buffer_is_NULL()
        {
            // Given
            var parser = new HttpParser(null);

            // When
            parser.ReadLine("abc");
            var remainder = parser.GetRemainder();

            // Then
            Assert.Null(remainder);
        }

        [Fact]
        public void Test_IS_Chunk_Compete_Should_Work_For_Given_Minimum_Chunk_Size()
        {
            // Given
            byte[] buffer = Encoding.UTF8.GetBytes("3\r\nabc\r\n2b");
            var parser = new HttpParser(buffer);

            // When
            parser.ReadLine(NewLine);
            parser = new HttpParser(parser.GetRemainder());
            byte[] byteLine = parser.ReadLine(NewLine);
            // Then
            Assert.True(parser.IsChunkComplete(byteLine, NewLine, 5));
        }

        [Fact]
        public void Test_IS_Chunk_Compete_Should_Work_For_Simple_Case()
        {
            // Given
            const string data = "12345";
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[5];
            stream.Read(buffer, 0, 5);
            var chunkReader = new HttpParser(buffer);

            // When
            byte[] byteLine = chunkReader.ReadLine(NewLine);

            // When, Then
            Assert.False(chunkReader.IsChunkComplete(byteLine, NewLine));
        }

        [Fact]
        public void Test_ReadLine_Should_Correctly_ReadLine_For_EmptyLINE()
        {
            // Given
            const string data = "\r\n\r\n";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 4);
            var chunkReader = new HttpParser(buffer);

            // When
            string line = Encoding.UTF8.GetString(chunkReader.ReadLine(EmptyLine));

            // Then
            Assert.Equal(data, line);
        }

        [Fact]
        public void Test_ReadLine_Should_Correctly_ReadLine_More_Complex_Case()
        {
            // Given
            const string data = "23\r\n\r\n\r\n2345";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            byte[] buffer = new byte[12];
            stream.Read(buffer, 0, 12);
            var chunkReader = new HttpParser(buffer);

            // When
            chunkReader.ReadLine(NewLine);
            chunkReader.ReadLine(EmptyLine);
            // Then
            Assert.Equal("2345", Encoding.ASCII.GetString(chunkReader.ReadLine(NewLine)));
        }

        [Fact]
        public void Test_ReadLine_Should_Correctly_ReadLine_Separator_Has_Not_Been_Found()
        {
            // Given
            const string data = "1234";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            var chunkReader = new HttpParser(buffer);

            // When
            string line = Encoding.ASCII.GetString(chunkReader.ReadLine(NewLine));
            // Then
            Assert.Equal("1234", line);
        }

        [Fact]
        public void Test_ReadLine_Should_Read_LeadingLine_For_EmptyLine_Separator()
        {
            // Given
            const string data = "322345\r\n\r\nab";
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[12];

            // When
            stream.Read(buffer, 0, 12);
            var chunkReader = new HttpParser(buffer);
            byte[] line = chunkReader.ReadLine(EmptyLine);

            // Then
            Assert.Equal("322345\r\n\r\n", Encoding.UTF8.GetString(line));
        }

        [Fact]
        public void Test_ReadLine_When_Buffer_is_NULL()
        {
            // Given
            var parser = new HttpParser(null);

            // When
            var read = parser.ReadLine("abc");

            // Then
            Assert.Null(read);
        }
    }
}