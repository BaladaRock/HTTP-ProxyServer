using ProxyServer;
using System.Text;
using Xunit;

namespace ProxyServer_Facts
{
    public class HttpReaderFacts
    {
        private const int GivenLength = 8;

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

        [Fact]
        public void Test_Read_Method_First_StreamRead_Does_NOT_Contain_Separator()
        {
            //Given
            const string data = "12345678abc\r\n\r\nandrei";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream, GivenLength);
            byte[] headers = reader.ReadHeaders();

            //Then
            Assert.Equal("12345678abc\r\n\r\n", Encoding.UTF8.GetString(headers));
        }
    }
}