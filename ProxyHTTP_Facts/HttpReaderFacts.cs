
using ProxyServer;
using System.Text;
using Xunit;

namespace ProxyServer_Facts
{
    public class HttpReaderFacts
    {
        [Fact]
        public void Test_Read_Method()
        {
            //Given
            const string data = "12345\r\n\r\n678";
            var stream = new StubNetworkStream(data);

            //When
            var reader = new HeadersReader(stream);
            byte[] headers = reader.ReadHeaders();

            //Then
            Assert.Equal("12345\r\n\r\n", Encoding.UTF8.GetString(headers));
        }

    }
}
