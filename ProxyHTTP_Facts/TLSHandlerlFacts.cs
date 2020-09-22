using ProxyServer;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ProxyHTTP_Facts
{
    public class TlsHandlerlFacts
    {
        [Fact]
        public void Test_Get_Successful_StatusMessage_For_Connect_Request()
        {
            //Given
            var tlsTunnel = new TlsHandler(null);

            //When
            byte[] response = tlsTunnel.GetSuccessResponse();

            //Then
            Assert.Equal("HTTP/1.1 200 OK", Encoding.UTF8.GetString(response));
        }

        [Fact]
        public void Test_Get_Failed_StatusMessage_For_Connect_Request()
        {
            //Given
            var tlsTunnel = new TlsHandler(null);

            //When
            byte[] response = tlsTunnel.GetFailureResponse();

            //Then
            Assert.Equal("503 Service Unavailable", Encoding.UTF8.GetString(response));
        }
    }
}
