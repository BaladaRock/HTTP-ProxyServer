﻿using ProxyServer;
using System.Text;
using Xunit;

namespace ProxyHTTP_Facts
{
    public class ContentLengthFacts
    {
        private const string TenBytes = "1234567890";

        [Fact]
        public void Test_ReadFromStream_Given_BodyPart_is_Sufficient()
        {
            //Given
            byte[] body = Encoding.UTF8.GetBytes("abcd");
            var stream = new StubNetworkStream(TenBytes);
            var contentHandler = new ContentLength(stream, stream);

            //When
            contentHandler.HandleResponseBody(body, "4");
            byte[] read = stream.GetReadBytes;

            //Then
            Assert.Null(read);
        }

        [Fact]
        public void Test_ReadFromStream_When_One_Read_Should_Be_Invoked()
        {
            //Given
            byte[] body = Encoding.UTF8.GetBytes("abcd");
            var stream = new StubNetworkStream(TenBytes);
            var contentHandler = new ContentLength(stream, stream);

            //When
            contentHandler.HandleResponseBody(body, " 10 ");
            byte[] readFromStream = stream.GetReadBytes;

            //Then
            Assert.Equal("123456", Encoding.UTF8.GetString(readFromStream));
        }

        [Fact]
        public void Test_WriteOnStream_BodyLength_Is_Smaller_Than_DataLength()
        {
            //Given
            var stream = new StubNetworkStream(TenBytes);
            var contentHandler = new ContentLength(stream, stream);

            //When
            contentHandler.HandleResponseBody(null, " 4");
            byte[] writtenToStream = stream.GetWrittenBytes;

            //Then
            Assert.Equal("1234", Encoding.UTF8.GetString(writtenToStream));
        }

        [Fact]
        public void Test_WriteOnStream_ContentLengthValue_Larger_Than_StreamData()
        {
            //Given
            byte[] body = Encoding.UTF8.GetBytes("abcd");
            var stream = new StubNetworkStream("123456789abcdefghijklmno");
            var contentHandler = new ContentLength(stream, stream);

            //When
            contentHandler.HandleResponseBody(body, "37");
            byte[] writtenToStream = stream.GetWrittenBytes;

            //Then
            Assert.Equal("abcd123456789abcdefghijklmno", Encoding.UTF8.GetString(writtenToStream));
        }

        [Fact]
        public void Test_WriteOnStream_Longer_Data()
        {
            //Given
            const string data = "123456789";
            var stream = new StubNetworkStream(data);
            var contentHandler = new ContentLength(stream, stream);

            //When
            contentHandler.HandleResponseBody(null, "9");
            byte[] writtenToStream = stream.GetWrittenBytes;

            //Then
            Assert.Equal("123456789", Encoding.UTF8.GetString(writtenToStream));
        }

        [Fact]
        public void Test_WriteOnStream_Should_Prepend_BodyPart_To_WrittenBytes()
        {
            //Given
            byte[] body = Encoding.UTF8.GetBytes("abcd");
            var stream = new StubNetworkStream(TenBytes);
            var contentHandler = new ContentLength(stream, stream);

            //When
            contentHandler.HandleResponseBody(body, "10");
            byte[] writtenToStream = stream.GetWrittenBytes;

            //Then
            Assert.Equal("abcd123456", Encoding.UTF8.GetString(writtenToStream));
        }

        [Fact]
        public void Test_WriteOnStream_Should_ReadCorrecly_for_given_BodyPart()
        {
            //Given
            byte[] body = Encoding.UTF8.GetBytes("abcd");
            var stream = new StubNetworkStream(TenBytes);
            var contentHandler = new ContentLength(stream, stream);

            //When
            contentHandler.HandleResponseBody(body, "4");
            byte[] writtenToStream = stream.GetWrittenBytes;

            //Then
            Assert.Equal("abcd", Encoding.UTF8.GetString(writtenToStream));
        }

        [Fact]
        public void Test_WriteOnStream_Should_Return_NULL_When_BodyLength_is_ZERO()
        {
            //Given
            var stream = new StubNetworkStream(TenBytes);
            var contentHandler = new ContentLength(stream, stream);

            //When
            contentHandler.HandleResponseBody(null, "0 ");
            byte[] writtenToStream = stream.GetWrittenBytes;

            //Then
            Assert.Null(writtenToStream);
        }

        [Fact]
        public void Test_WriteOnStream_Simple_Data()
        {
            //Given
            const string data = "1234";
            var stream = new StubNetworkStream(data);
            var contentHandler = new ContentLength(stream, stream);

            //When
            contentHandler.HandleResponseBody(null, "4");
            byte[] writtenToStream = stream.GetWrittenBytes;

            //Then
            Assert.Equal("1234", Encoding.UTF8.GetString(writtenToStream));
        }

        [Fact]
        public void Test_GetRemainder_BodyPart_is_Sufficient()
        {
            //Given
            byte[] body = Encoding.UTF8.GetBytes("abcdef");
            var stream = new StubNetworkStream("abcdefghijklmno");
            var contentHandler = new ContentLength(stream, stream);

            //When
            contentHandler.HandleResponseBody(body, "5");
            byte[] remainder = contentHandler.Remainder;

            //Then
            Assert.Equal("f", Encoding.UTF8.GetString(remainder));
        }

        [Fact]
        public void Test_GetRemainder_BodyPartLength_Equals_TotalBodyLength()
        {
            //Given
            byte[] body = Encoding.UTF8.GetBytes("abcdef");
            var stream = new StubNetworkStream("abcdefghijklmno");
            var contentHandler = new ContentLength(stream, stream);

            //When
            contentHandler.HandleResponseBody(body, "6");
            byte[] remainder = contentHandler.Remainder;

            //Then
            Assert.Null(remainder);
        }

        [Fact]
        public void Test_GetRemainder_NoRemainder_When_BodyPart_is_NOT_Sufficient()
        {
            //Given
            byte[] body = Encoding.UTF8.GetBytes("abcd");
            var stream = new StubNetworkStream(TenBytes);
            var contentHandler = new ContentLength(stream, stream);

            //When
            contentHandler.HandleResponseBody(body, "10");
            byte[] remainder = contentHandler.Remainder;

            //Then
            Assert.Null(remainder);
        }
    }
}