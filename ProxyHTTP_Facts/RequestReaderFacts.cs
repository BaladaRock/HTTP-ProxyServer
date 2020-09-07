using ProxyServer;
using Xunit;

namespace ProxyHTTP_Facts
{
    public class RequestReaderFacts
    {
        [Fact]
        public void CONNECT_Should_be_FALSE_Different_Request_Type()
        {
            //Given
            const string request = "GET www.dacos.com.ro/favicon.ico HTTP/1.1";
            var reader = new RequestReader(request);

            //When
            reader.CheckConnect();

            //Then
            Assert.False(reader.IsConnect);
        }

        [Fact]
        public void CONNECT_Should_be_TRUE_for_SIMPLE_Request()
        {
            //Given
            const string request = "CONNECT www.google-analytics.com:443 HTTP/1.1";
            var reader = new RequestReader(request);

            //When
            reader.CheckConnect();

            //Then
            Assert.True(reader.IsConnect);
        }

        [Fact]
        public void CONNECT_Should_be_TRUE_Request_Has_EmptySpaces()
        {
            //Given
            const string request = " CONNECT www.google-analytics.com:443 HTTP/1.1 ";
            var reader = new RequestReader(request);

            //When
            reader.CheckConnect();

            //Then
            Assert.True(reader.IsConnect);
        }

        [Fact]
        public void GET_Should_be_TRUE_for_SIMPLE_Request()
        {
            //Given
            const string request = "GET www.dacos.com.ro/favicon.ico HTTP/1.1";
            var reader = new RequestReader(request);

            //When
            reader.CheckConnect();

            //Then
            Assert.True(reader.IsGet);
        }

        [Fact]
        public void GET_Should_be_FALSE_Different_Request()
        {
            //Given
            const string request = " CONNECT www.google-analytics.com:443 HTTP/1.1 ";
            var reader = new RequestReader(request);

            //When
            reader.CheckConnect();

            //Then
            Assert.False(reader.IsGet);
        }

        [Fact]
        public void Test_Port_Should_correctly_Extract_443_PORT_simple_case()
        {
            //Given
            const string request = "CONNECT www.google-analytics.com:443 HTTP/1.1 ";
            var reader = new RequestReader(request);

            //When
            int port = reader.GetPort();

            //Then
            Assert.Equal(443, port);
        }

        [Fact]
        public void Should_Correctly_Extract_HOST_Simple_Case_GET_Request()
        {
            //Given
            const string request = "GET something\r\nHeader\r\nHost: Andrei\r\n";
            var reader = new RequestReader(request);

            //When
            string host = reader.Host;

            //Then
            Assert.Equal("Andrei", host);
        }

        [Fact]
        public void Should_Correctly_Extract_HOST_PORT_CONNECT_Request()
        {
            //Given
            const string request = "CONNECT something\r\nHeader\r\nHost: Andrei\r\n";
            var reader = new RequestReader(request);

            //When
            string host = reader.Host;

            //Then
            Assert.Equal("Andrei", host);
        }

        [Fact]
        public void Test_HOST_When_HOSTHEADER_appears_Later_duirng_Request()
        {
            //Given
            const string request = "Header1\r\nHeader2\r\nHost: Andrei\r\nHeader3\r\n";
            var reader = new RequestReader(request);

            //When
            string host = reader.Host;

            //Then
            Assert.Equal("Andrei", host);
        }

        [Fact]
        public void Test_HOST_Should_Return_NULL_When_NO_Host_was_FOUND()
        {
            //Given
            const string request = "Header1\r\nHeader2\r\nHos: Andrei\r\nHeader3\r\n";
            var reader = new RequestReader(request);

            //When
            string host = reader.Host;

            //Then
            Assert.Null(host);
        }
    }
}