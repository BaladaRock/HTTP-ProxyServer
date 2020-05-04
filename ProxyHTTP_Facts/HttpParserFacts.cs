using ProxyHTTP;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ProxyHTTP_Facts
{
    public class HttpParserFacts
    {
        private const string EmptyLine = Headers.EmptyLine;
        private const string NewLine = Headers.NewLine;

        [Fact]
        public void Test_Contains_Method_Array_Does_NOT_Contain_SubArray()
        {
            // Given
            const string data = "abcd\r\n\r\nab";
            const string subArray = "\r\r";

            // When
            var parser = new HttpParser(Encoding.UTF8.GetBytes(data));
            byte[] readLine = parser.ReadLine(EmptyLine);

            // Then
            Assert.False(parser.Contains(readLine, Encoding.UTF8.GetBytes(subArray)));
        }

        [Fact]
        public void Test_Contains_Method_SubArray_is_at_THE_END()
        {
            // Given
            const string data = "abcd\r\n\r\nab";
            const string subArray = "\nab";

            // When
            var parser = new HttpParser(Encoding.UTF8.GetBytes(data));
            byte[] readLine = parser.ReadLine("\r\n\r\n\r\n");

            // Then
            Assert.Equal(10, parser.GetPosition(readLine, Encoding.UTF8.GetBytes(subArray)));
            Assert.True(parser.Contains(readLine, Encoding.UTF8.GetBytes(subArray)));
        }

        [Fact]
        public void Test_ContainsMethod_Should_Know_If_A_Response_Contains_EMPTY_LINE()
        {
            // Given
            const string data = "abcd\r\n\r\nab";

            // When
            var parser = new HttpParser(Encoding.UTF8.GetBytes(data));
            byte[] readLine = parser.ReadLine(EmptyLine);

            // Then
            Assert.True(parser.Contains(readLine, Headers.EmptyLineBytes));
        }

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
        public void Test_IS_Chunk_Compete_Should_Work_For_Given_Minimum_Chunk_Size()
        {
            // Given
            const string data = "3\r\nabc\r\n2b";
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[10];
            stream.Read(buffer, 0, 10);
            var parser = new HttpParser(buffer);

            // When
            string line = Encoding.UTF8.GetString(parser.ReadLine(NewLine));
            byte[] byteLine = parser.ReadBytes(line);
            int size = Convert.ToInt32(line);
            // Then
            Assert.True(parser.IsChunkComplete(byteLine, NewLine, size));
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
            byte[] byteLine = chunkReader.ReadBytes("5\r\n");

            // When, Then
            Assert.False(chunkReader.IsChunkComplete(byteLine, NewLine));
        }

        [Fact]
        public void Test_Read_Bytes_From_ReadLine_Value_Buffer_Has_More_Elements()
        {
            // Given
            const string data = "3\r\nabc\r\naaaaa";
            byte[] toCheck = Encoding.UTF8.GetBytes("abc\r\n");

            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[24];
            stream.Read(buffer, 0, 24);

            var chunkReader = new HttpParser(buffer);

            // When
            string line = Encoding.UTF8.GetString(chunkReader.ReadLine(NewLine));
            byte[] byteLine = chunkReader.ReadBytes(line);

            // Then
            Assert.True(byteLine.SequenceEqual(toCheck));
        }

        [Fact]
        public void Test_Read_Bytes_Should_Read_correctly_for_ReadValue()
        {
            // Given
            const string data = "3\r\nabc\r\n";
            byte[] toCheck = Encoding.UTF8.GetBytes("abc\r\n");

            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 8);

            var chunkReader = new HttpParser(buffer);

            // When
            string line = Encoding.UTF8.GetString(chunkReader.ReadLine(NewLine));
            byte[] byteLine = chunkReader.ReadBytes(line);

            // Then
            Assert.True(byteLine.SequenceEqual(toCheck));
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
            var chunkReader = new HttpParser(buffer);
            chunkReader.ReadLine(NewLine);
            chunkReader.ReadLine(EmptyLine);
            string line = Encoding.UTF8.GetString(chunkReader.ReadLine(NewLine));
            string byteLine = Encoding.UTF8.GetString(chunkReader.ReadBytes(line));

            // Then
            Assert.Equal("222\r\n", byteLine);
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
    }
}