using System;
using Xunit;
using ProxyServer;
using ProxyHTTP;
using System.Text;

namespace ProxyHTTP_Facts
{
    public class ChunkedEncodingFacts
    {
        private const int Eight = 8;

        [Fact]
        public void Test_ReadBytes_For_GivenSize_NoRemainder_After_ReadingHeaders()
        {
            //Given
            const string data = "1234567890";
            var stream = new StubNetworkStream(data);
            var chunked = new ChunkedEncoding(stream, stream);

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
            var chunked = new ChunkedEncoding(stream, stream);

            //When
            chunked.ReadAndSendBytes(Eight, remainder);

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
            var chunked = new ChunkedEncoding(stream, stream);

            //When
            chunked.ReadAndSendBytes(Eight, remainder);

            //Then
            Assert.Null(stream.GetReadBytes);
        }

        [Fact]
        public void Test_SentBytes_NoRemainder_After_ReadingHeaders()
        {
            //Given
            const string data = "1234567890";
            var stream = new StubNetworkStream(data);
            var chunked = new ChunkedEncoding(stream, stream);

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
            var chunked = new ChunkedEncoding(stream, stream);

            //When
            chunked.ReadAndSendBytes(Eight, remainder);

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
            var chunked = new ChunkedEncoding(stream, stream);

            //When
            chunked.ReadAndSendBytes(Eight, remainder);

            //Then
            Assert.Equal("abcdefgh", Encoding.UTF8.GetString(stream.GetWrittenBytes));
        }

        [Fact]
        public void Test_ConvertFromHexadecimal_Should_Correctly_Convert_SmallNumber()
        {
            //Given
            const string hexa = "4";
            var chunkHandler = new ChunkedEncoding(null, null);

            //When
            int bodyLength = chunkHandler.ConvertFromHexadecimal(hexa);

            //Then
            Assert.Equal(4, bodyLength);
        }

        [Fact]
        public void Test_ConvertFromHexadecimal_String_Has_Hexadecimal_Symbols()
        {
            //Given
            const string hexa = "AB1";
            var chunkHandler = new ChunkedEncoding(null, null);

            //When
            int bodyLength = chunkHandler.ConvertFromHexadecimal(hexa);

            //Then
            Assert.Equal(2737, bodyLength);
        }

        [Fact]
        public void Test_ConvertFromHexadecimal_Should_Correctly_Convert_LargerNumber()
        {
            //Given
            const string hexa = "7AB45";
            var chunkHandler = new ChunkedEncoding(null, null);

            //When
            int bodyLength = chunkHandler.ConvertFromHexadecimal(hexa);

            //Then
            Assert.Equal(502597, bodyLength);
        }

        [Fact]
        public void Test_ConvertFromHexadecimal_String_Has_Whitespaces()
        {
            //Given
            const string hexa = " 40 ";
            var chunkHandler = new ChunkedEncoding(null, null);

            //When
            int bodyLength = chunkHandler.ConvertFromHexadecimal(hexa);

            //Then
            Assert.Equal(64, bodyLength);
        }

        [Fact]
        public void Test_ChunkHandling_SentBytes_For_One_Chunk()
        {
            //Given
            const string data = "0\r\n\r\n1234";
            var stream = new StubNetworkStream(data);
            var chunkHandler = new ChunkedEncoding(stream, stream);

            //When
            chunkHandler.HandleChunked();

            //Then
            Assert.Null(stream.GetWrittenBytes);
        }

        [Fact]
        public void Test_ChunkHandling_SentBytes_For_Two_Chunks()
        {
            //Given
            const string data = "4\r\n1234\r\n0\r\n\r\n1234";
            var stream = new StubNetworkStream(data);
            var chunkHandler = new ChunkedEncoding(stream, stream);

            //When
            chunkHandler.HandleChunked();

            //Then
            Assert.Equal("1234", Encoding.UTF8.GetString(stream.GetWrittenBytes));
        }

        [Fact]
        public void Test_ChunkHandling_Assure_ChunkSize_is_read_as_HEXADECIMAL()
        {
            //Given
            const string data = "A\r\n123\r\n45678\r\n0\r\n\r\n1234";
            var stream = new StubNetworkStream(data);
            var chunkHandler = new ChunkedEncoding(stream, stream);

            //When
            chunkHandler.HandleChunked();

            //Then
            Assert.Equal("123\r\n45678", Encoding.UTF8.GetString(stream.GetWrittenBytes));
        }

        [Fact]
        public void Test_ChunkHandling_RepetiveProcess_For_More_Chunks()
        {
            //Given
            const string data = "2 \r\nab\r\n4\r\n1234\r\n0\r\n\r\n1234";
            var stream = new StubNetworkStream(data);
            var chunkHandler = new ChunkedEncoding(stream, stream);

            //When
            chunkHandler.HandleChunked();

            //Then
            Assert.Equal("ab1234", Encoding.UTF8.GetString(stream.GetWrittenBytes));
        }
    }
}
