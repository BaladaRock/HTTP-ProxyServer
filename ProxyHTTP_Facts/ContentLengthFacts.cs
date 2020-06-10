using ProxyServer;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ProxyHTTP_Facts
{
    public class ContentLengthFacts
    {
        [Fact]
        public void Test_ContentLength_Should_Write_On_Stream_Simple_Data()
        {
            //Given
            const string data = "1234";
            var stream = new StubNetworkStream(data);
            var contentHandler = new ContentLength(stream, null, 0);

            //When
            contentHandler.HandleResponseBody();
            byte[] writtenToStream = stream.GetWrittenBytes;

            //Then
            Assert.Equal("1234", Encoding.UTF8.GetString(writtenToStream));
        }



    }
}
