using ProxyServer;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ProxyHTTP_Facts
{
    public class ContentLengthFacts
    {
        private const string TenBytes = "123456789a";

        [Fact]
        public void Test_WriteOnStream_Simple_Data()
        {
            //Given
            const string data = "1234";
            var stream = new StubNetworkStream(data);
            var contentHandler = new ContentLength(stream, stream, null, 4);

            //When
            contentHandler.HandleResponseBody();
            byte[] writtenToStream = stream.GetWrittenBytes;

            //Then
            Assert.Equal("1234", Encoding.UTF8.GetString(writtenToStream));
        }

        [Fact]
        public void Test_WriteOnStream_Longer_Data()
        {
            //Given
            const string data = "123456789";
            var stream = new StubNetworkStream(data);
            var contentHandler = new ContentLength(stream, stream, null, 9);

            //When
            contentHandler.HandleResponseBody();
            byte[] writtenToStream = stream.GetWrittenBytes;

            //Then
            Assert.Equal("123456789", Encoding.UTF8.GetString(writtenToStream));
        }

        [Fact]
        public void Test_WriteOnStream_BodyLength_Is_Smaller_Than_DataLength()
        {
            //Given
            var stream = new StubNetworkStream(TenBytes);
            var contentHandler = new ContentLength(stream, stream, null, 4);

            //When
            contentHandler.HandleResponseBody();
            byte[] writtenToStream = stream.GetWrittenBytes;

            //Then
            Assert.Equal("1234", Encoding.UTF8.GetString(writtenToStream));
        }

        [Fact]
        public void Test_WriteOnStream_Should_Return_NULL_When_BodyLength_is_ZERO()
        {
            //Given
            var stream = new StubNetworkStream(TenBytes);
            var contentHandler = new ContentLength(stream, stream, null, 0);

            //When
            contentHandler.HandleResponseBody();
            byte[] writtenToStream = stream.GetWrittenBytes;

            //Then
            Assert.Null(writtenToStream);
        }

        [Fact]
        public void Test_WriteOnStream_Should_ReadCorrecly_for_given_BodyPart()
        {
            //Given
            byte[] body = Encoding.UTF8.GetBytes("abcd");
            var stream = new StubNetworkStream(TenBytes);
            var contentHandler = new ContentLength(stream, stream, body, 4);

            //When
            contentHandler.HandleResponseBody();
            byte[] writtenToStream = stream.GetWrittenBytes;

            //Then
            Assert.Equal("abcd", Encoding.UTF8.GetString(writtenToStream));
        }

        [Fact]
        public void Test_WriteOnStream_Should_Prepend_BodyPart_To_WrittenBytes()
        {
            //Given
            byte[] body = Encoding.UTF8.GetBytes("abcd");
            var stream = new StubNetworkStream(TenBytes);
            var contentHandler = new ContentLength(stream, stream, body, 10);

            //When
            contentHandler.HandleResponseBody();
            byte[] writtenToStream = stream.GetWrittenBytes;

            //Then
            Assert.Equal("abcd123456", Encoding.UTF8.GetString(writtenToStream));
        }

        [Fact]
        public void Test_Larger_Value_Should_Work_Correctly_For_Repetitive_StreamCalls()
        {
            //Given
            byte[] body = Encoding.UTF8.GetBytes("abcd");
            var stream = new StubNetworkStream("123456789abcdefghijklmno");
            var contentHandler = new ContentLength(stream, stream, body, 25);

            //When
            contentHandler.HandleResponseBody();
            byte[] writtenToStream = stream.GetWrittenBytes;

            //Then
            Assert.Equal("abcd123456789abcdefghijkl", Encoding.UTF8.GetString(writtenToStream));
        }

    }
}
