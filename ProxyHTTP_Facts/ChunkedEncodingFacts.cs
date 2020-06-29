using System;
using Xunit;
using ProxyServer;
using ProxyHTTP;
using System.Text;

namespace ProxyHTTP_Facts
{
    public class ChunkedEncodingFacts
    {
        private const string Eight = "8";

        [Fact]
        public void Test_ReadBytes_For_GivenSize_NoRemainder_After_ReadingHeaders()
        {
            //Given
            const string data = "1234567890";
            var stream = new StubNetworkStream(data);
            var chunked = new ChunkedEncoding(stream, stream, null);

            //When
            chunked.ReadAndSendBytes(Eight);

            //Then
            Assert.Equal("12345678", Encoding.UTF8.GetString(stream.GetReadBytes));
        }

        [Fact]
        public void Test_ReadBytes_For_GivenSize_WITH_Remainder()
        {
            //Given
            const string data = "1234567890";
            byte[] remainder = Encoding.UTF8.GetBytes("abcd");
            var stream = new StubNetworkStream(data);
            var chunked = new ChunkedEncoding(stream, stream, remainder);

            //When
            chunked.ReadAndSendBytes(Eight);

            //Then
            Assert.Equal("1234", Encoding.UTF8.GetString(stream.GetReadBytes));
        }

        [Fact]
        public void Test_ReadBytes_For_GivenSize_WITH_Larger_Remainder()
        {
            //Given
            const string data = "1234567890";
            byte[] remainder = Encoding.UTF8.GetBytes("abcdefgh");
            var stream = new StubNetworkStream(data);
            var chunked = new ChunkedEncoding(stream, stream, remainder);

            //When
            chunked.ReadAndSendBytes(Eight);

            //Then
            Assert.Null(stream.GetReadBytes);
        }

        [Fact]
        public void Test_SentBytes_NoRemainder_After_ReadingHeaders()
        {
            //Given
            const string data = "1234567890";
            var stream = new StubNetworkStream(data);
            var chunked = new ChunkedEncoding(stream, stream, null);

            //When
            chunked.ReadAndSendBytes(Eight);

            //Then
            Assert.Equal("12345678", Encoding.UTF8.GetString(stream.GetWrittenBytes));
        }

        [Fact]
        public void Test_SentBytes_When_There_IS_Remainder()
        {
            //Given
            const string data = "1234567890";
            byte[] remainder = Encoding.UTF8.GetBytes("abcd");
            var stream = new StubNetworkStream(data);
            var chunked = new ChunkedEncoding(stream, stream, remainder);

            //When
            chunked.ReadAndSendBytes(Eight);

            //Then
            Assert.Equal("abcd1234", Encoding.UTF8.GetString(stream.GetWrittenBytes));
        }

        [Fact]
        public void Test_SentBytes_With_Sufficient_and_Larger_Remainder()
        {
            //Given
            const string data = "1234567890";
            byte[] remainder = Encoding.UTF8.GetBytes("abcdefghij");
            var stream = new StubNetworkStream(data);
            var chunked = new ChunkedEncoding(stream, stream, remainder);

            //When
            chunked.ReadAndSendBytes(Eight);

            //Then
            Assert.Equal("abcdefgh", Encoding.UTF8.GetString(stream.GetWrittenBytes));
        }
        /* [Fact]
         public void Test_ConvertFromHexadecimal_Should_Correctly_Convert_SmallNumber()
         {
             //Given
             const string hexa = "4";
             var contentHandler = new ContentLength(null, null);

             //When
             int bodyLength = contentHandler.ConvertFromHexadecimal(hexa);

             //Then
             Assert.Equal(4, bodyLength);
         }

         [Fact]
         public void Test_ConvertFromHexadecimal_String_Has_Hexadecimal_Symbols()
         {
             //Given
             const string hexa = "AB1";
             var contentHandler = new ContentLength(null, null);

             //When
             int bodyLength = contentHandler.ConvertFromHexadecimal(hexa);

             //Then
             Assert.Equal(2737, bodyLength);
         }

         [Fact]
         public void Test_ConvertFromHexadecimal_Should_Correctly_Convert_LargerNumber()
         {
             //Given
             const string hexa = "7AB45";
             var contentHandler = new ContentLength(null, null);

             //When
             int bodyLength = contentHandler.ConvertFromHexadecimal(hexa);

             //Then
             Assert.Equal(502597, bodyLength);
         }

         [Fact]
         public void Test_ConvertFromHexadecimal_String_Has_Whitespaces()
         {
             //Given
             const string hexa = " 40 ";
             var contentHandler = new ContentLength(null, null);

             //When
             int bodyLength = contentHandler.ConvertFromHexadecimal(hexa);

             //Then
             Assert.Equal(64, bodyLength);

             internal int ConvertFromHexadecimal(string hexa)
             {
                 return Convert.ToInt32(hexa.Trim(), 16);
             }
         }*/
    }
}
