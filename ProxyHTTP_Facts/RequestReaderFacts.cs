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

        [Fact]
        public void Connect_Should_be_TRUE_Request_Has_EmptySpaces()
        {
            //Given
            const string request = " CONNECT www.google-analytics.com:443 HTTP/1.1 ";
            var reader = new RequestReader(request);

            //When
            reader.CheckRequest();

            //Then
            Assert.True(reader.Connect);
        }

        [Fact]
        public void Connect_Should_be_FALSE_Different_Request_Type()
        {
            //Given
            const string request = "GET www.dacos.com.ro/favicon.ico HTTP/1.1";
            var reader = new RequestReader(request);

            //When
            reader.CheckRequest();

            //Then
            Assert.False(reader.Connect);
        }

        [Fact]
        public void Test_Port_Should_correctly_Extract_PORT_simple_case()
        {
            //Given
            const string request = "CONNECT www.google-analytics.com:443 HTTP/1.1 ";
            var reader = new RequestReader(request);

            //When
            int port = reader.GetPort();

            //Then
            Assert.Equal(443, port);
        }

    }
}
