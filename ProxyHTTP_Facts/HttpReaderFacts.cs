
using ProxyServer;
using System.Text;
using Xunit;

namespace ProxyHTTP_Facts
{
    public class HttpReaderFacts
    {
        [Fact]
        public void Test_Read_Method()
        {
            //Given
            const string data = "12345\r\n\r\n678";
            var buffer = new byte[24];
            var stream = new StubNetworkStream(data);
            int readFromStream = stream.Read(buffer, 0, buffer.Length);

            //When
            var reader = new HeadersReader(stream);
            byte[] headers = reader.ReadHeaders();

            //Then
            Assert.Equal("12345\r\n\r\n", Encoding.UTF8.GetString(headers));
        }
         

    }
}
