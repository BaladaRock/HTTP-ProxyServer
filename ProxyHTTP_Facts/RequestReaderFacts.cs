using ProxyHTTP;
using ProxyServer;
using Xunit;

namespace ProxyHTTP_Facts
{
    public class RequestReaderFacts
    {
        [Fact]
        public void Connect_Should_be_TRUE_for_SIMPLE_Request()
        {
            //Given
            const string request = "CONNECT www.google-analytics.com:443 HTTP/1.1";
            var reader = new RequestReader(request);
            //When
            reader.CheckRequest();
            //Then
            Assert.True(reader.Connect);
        }



    }
}
