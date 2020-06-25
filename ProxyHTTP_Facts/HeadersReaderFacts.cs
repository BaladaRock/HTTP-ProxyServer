using ProxyServer;
using System.Text;
using Xunit;

namespace ProxyHTTP_Facts
{
    public class HeadersReaderFacts
    {
        private const int Eight = 8;
        private const int Four = 4;
        private const int Ten = 10;
        private const int Three = 3;
        private const int Two = 2;

        [Fact]
        public void Should_Correctly_ReadHeaders_EdgeCase_With_MULTIPLE_Reads()
        {
            //Given
            const string data = "12\r\n123\r\n1234\r\n\r\nandrei";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Two);
            byte[] headers = reader.ReadHeaders();

            //Then
            Assert.Equal("12\r\n123\r\n1234\r\n\r\n", Encoding.UTF8.GetString(headers));
        }

        [Fact]
        public void Should_Do_Multiple_reads_Firstread_PART_Does_NOT_Contain_Separator()
        {
            //Given
            const string data = "12345678abc\r\n\r\nandrei";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Eight);
            byte[] headers = reader.ReadHeaders();

            //Then
            Assert.Equal("12345678abc\r\n\r\n", Encoding.UTF8.GetString(headers));
        }

        [Fact]
        public void Test_GetRemainder_After_Read_Simple_Case()
        {
            //Given
            const string data = "1234\r\n\r\nandrei";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Ten);
            reader.ReadHeaders();
            byte[] remainder = reader.Remainder;

            //Then
            Assert.Equal("an", Encoding.UTF8.GetString(remainder));
        }

        [Fact]
        public void Test_GetRemainder_For_More_Complex_Case()
        {
            //Given
            const string data = "12\r\n123\r\n1234\r\n\r\nandrei";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Two);
            reader.ReadHeaders();
            byte[] remainder = reader.Remainder;

            //Then
            Assert.Equal("a", Encoding.UTF8.GetString(remainder));
        }

        [Fact]
        public void Test_GetRemainder_is_Smaller_Than_Total_StreamData_MultipleHeaders()
        {
            //Given
            const string data = "12\r\n1234\r\n12345\r\n";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, 3);
            reader.ReadHeaders();
            byte[] remainder = reader.Remainder;

            //Then
            Assert.Null(remainder);
        }

        [Fact]
        public void Test_GetRemainder_ReadSize_Overtakes_StreamData_more_Complex_Case()
        {
            //Given
            const string data = "12\r\n34\r\n\r\naa";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Eight);
            reader.ReadHeaders();
            byte[] remainder = reader.Remainder;

            //Then
            Assert.Equal("aa", Encoding.UTF8.GetString(remainder));
        }

        [Fact]
        public void Test_GetRemainder_ReadSize_OverTakes_Total_StreamData()
        {
            //Given
            const string data = "1234\r\n\r\nandrei";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, 7);
            reader.ReadHeaders();
            byte[] remainder = reader.Remainder;

            //Then
            Assert.Equal("andrei", Encoding.UTF8.GetString(remainder));
        }

        [Fact]
        public void Test_GetRemainder_ReadValue_Smaller_Than_StreamData_MultipleHeaders()
        {
            //Given
            const string data = "12\r\n1234\r\n12345\r\n\r\nab";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Three);
            reader.ReadHeaders();
            byte[] remainder = reader.Remainder;

            //Then
            Assert.Equal("ab", Encoding.UTF8.GetString(remainder));
        }

        [Fact]
        public void Test_GetRemainder_Should_Return_NULL_After_Multiple_Reads()
        {
            //Given
            const string data = "1234nandrei\r\n\r\n";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Eight);
            reader.ReadHeaders();
            byte[] remainder = reader.Remainder;

            //Then
            Assert.Null(remainder);
        }

        [Fact]
        public void Test_GetRemainder_Should_Return_NULL_For_EdgeCase()
        {
            //Given
            const string data = "1234\r\nandrei\r\n\r\n";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Two);
            reader.ReadHeaders();
            byte[] remainder = reader.Remainder;

            //Then
            Assert.Null(remainder);
        }

        [Fact]
        public void Test_GetRemainder_Should_Return_Null_When_Separator_Is_Not_Found()
        {
            //Given
            const string data = "1234andrei";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Eight);
            reader.ReadHeaders();
            byte[] remainder = reader.Remainder;

            //Then
            Assert.Null(remainder);
        }

        [Fact]
        public void Test_GetRemainder_SmallerSize_Read()
        {
            //Given
            const string data = "1234\r\n\r\nandrei";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Three);
            byte[] headers = reader.ReadHeaders();
            byte[] remainder = reader.Remainder;

            //Then
            Assert.Equal("1234\r\n\r\n", Encoding.UTF8.GetString(headers));
            Assert.Equal("a", Encoding.UTF8.GetString(remainder));
        }

        [Fact]
        public void Test_ReadHeaders_EndLine_Is_AtTheEnd()
        {
            //Given
            const string data = "12345678\r\n\r\n";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, data.Length);
            byte[] headers = reader.ReadHeaders();

            //Then
            Assert.Equal("12345678\r\n\r\n", Encoding.UTF8.GetString(headers));
        }

        [Fact]
        public void Test_ReadHeaders_For_Simple_Case()
        {
            //Given
            const string data = "12345\r\n\r\n678";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, data.Length);
            byte[] headers = reader.ReadHeaders();

            //Then
            Assert.Equal("12345\r\n\r\n", Encoding.UTF8.GetString(headers));
        }

        [Fact]
        public void Test_ReadHeaders_Headers_are_Stored_Completely_after_Multiple_Reads()
        {
            //Given
            const string data = "12\r\n34\r\n5678\r\n\r\n";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, data.Length);
            byte[] headers = reader.ReadHeaders();

            //Then
            Assert.Equal(data, Encoding.UTF8.GetString(headers));
        }

        [Fact]
        public void Test_ReadHeaders_NULL_After_Reading_MoreBytes_than_StreamData()
        {
            //Given
            const string data = "1234andrei";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Eight);
            byte[] headers = reader.ReadHeaders();

            //Then
            Assert.Null(headers);
        }

        [Fact]
        public void Test_ReadHeaders_ReadValue_Smaller_Than_StreamData_MultipleHeaders()
        {
            //Given
            const string data = "12\r\n1234\r\n12345\r\n";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Three);
            byte[] headers = reader.ReadHeaders();

            //Then
            Assert.Null(headers);
        }

        [Fact]
        public void Test_ReadHeaders_Should_Return_Null_When_Separator_Is_Not_Found()
        {
            //Given
            const string data = "1234andrei";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Two);
            byte[] headers = reader.ReadHeaders();

            //Then
            Assert.Null(headers);
        }

        [Fact]
        public void Test_ReadProcess_ReadSize_Overtakes_StreamData_OneHeader_WasFound()
        {
            //Given
            const string data = "12\r\n";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Four);
            byte[] headers = reader.ReadHeaders();

            //Then
            Assert.Null(headers);
        }

        [Fact]
        public void Test_ReadProcess_ReadSize_Overtakes_Total_StreamData_SecondTest()
        {
            //Given
            const string data = "1234\r\n\r\nandrei";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Eight);
            byte[] headers = reader.ReadHeaders();
            byte[] remainder = reader.Remainder;

            //Then
            Assert.Equal("1234\r\n\r\n", Encoding.UTF8.GetString(headers));
            Assert.Null(remainder);
        }

        [Fact]
        public void Test_ContentLength_Header_Should_Return_Correctly_Valid_Case()
        {
            //Given
            const string data = "12\r\ncontent-length: 100\r\n\r\n";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Eight);
            reader.ReadHeaders();

            //Then
            Assert.Equal(100, reader.ContentLength);
        }

        [Fact]
        public void Test_ContentLength_Header_Should_Return_Zero()
        {
            //Given
            const string data = "12\r\ncontent-length: 0\r\n\r\n";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Eight);
            reader.ReadHeaders();

            //Then
            Assert.Equal(0, reader.ContentLength);
        }

        [Fact]
        public void Test_ContentLength_Header_Should_Work_for_UpperCases()
        {
            //Given
            const string data = "12\r\nContent-Length: 10\r\n\r\n";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Eight);
            reader.ReadHeaders();

            //Then
            Assert.Equal(10, reader.ContentLength);
        }

        [Fact]
        public void Test_ContentLength_Header_Should_Return_Negative_InvalidValue()
        {
            //Given
            const string data = "12\r\nContent-Length: -10\r\n\r\n";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Eight);
            reader.ReadHeaders();

            //Then
            Assert.Equal(-1, reader.ContentLength);
        }
    }
}