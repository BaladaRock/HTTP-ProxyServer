using ProxyServer;
using System.Text;
using Xunit;

namespace ProxyServer_Facts
{
    public class HttpReaderFacts
    {
        private const int Eight = 8;
        private const int Seven = 7;
        private const int Ten = 10;
        private const int Two = 2;

        [Fact]
        public void Should_Correctly_ReadHeaders_EdgeCase_With_MULTIPLE_Reads()
        {
            //Given
            const string data = "1234\r\n\r\nandrei";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Two);
            byte[] headers = reader.ReadHeaders();

            //Then
            Assert.Equal("1234\r\n\r\n", Encoding.UTF8.GetString(headers));
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
            byte[] remainder = reader.GetRemainder();

            //Then
            Assert.Equal("an", Encoding.UTF8.GetString(remainder));
        }

        [Fact]
        public void Test_GetRemainder_GivenSize_is_Smaller_Than_Total_StreamData()
        {
            //Given
            const string data = "1234\r\n\r\nandrei";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Seven);
            reader.ReadHeaders();
            byte[] remainder = reader.GetRemainder();

            //Then
            Assert.Equal("andrei", Encoding.UTF8.GetString(remainder));
        }

        [Fact]
        public void Test_GetRemainder_Should_Return_Null_When_Separator_Is_Not_Found()
        {
            //Given
            const string data = "1234andrei";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Two);
            reader.ReadHeaders();
            byte[] remainder = reader.GetRemainder();

            //Then
            Assert.Null(Encoding.UTF8.GetString(remainder));
        }

        [Fact]
        public void Test_Read_Method_Should_Return_Null_When_Separator_Is_Not_Found()
        {
            //Given
            const string data = "1234andrei";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, Two);
            byte[] remainder = reader.ReadHeaders();

            //Then
            Assert.Null(Encoding.UTF8.GetString(remainder));
        }

        [Fact]
        public void Test_Read_Method_EndLine_Is_AtTheEnd()
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
        public void Test_Read_Method_For_Simple_Case()
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
    }
}