using ProxyServer;
using System.Text;
using Xunit;

namespace ProxyHTTP_Facts
{
    public class ChunkedEncodingFacts
    {
        private const string DummyData = "A";
        private const string TenBytes = "1234567890";

        [Fact]
        public void Test_ChunkHandling_AfterHeader_Follows_ZeroSize_Chunk()
        {
            //Given
            const string data = "\nab\r\n 3\r\n123\r\n0\r\nHeader\r\n\r\n";
            byte[] bodyPart = Encoding.UTF8.GetBytes("2\r");
            var stream = new StubNetworkStream(data);
            var chunkHandler = new ChunkedEncoding(stream, stream);

            //When
            chunkHandler.HandleChunked(bodyPart);

            //Then
            Assert.Equal("ab123Header\r\n\r\n",
                     Encoding.UTF8.GetString(stream.GetWrittenBytes));
        }

        [Fact]
        public void Test_ChunkHandling_FirstBytesRead_is_NOT_Complete()
        {
            //Given
            const string data = "b\r\n4\r\n1234\r\n 3\r\n123\r\n0\r\n\r\n";
            byte[] bodyPart = Encoding.UTF8.GetBytes("2 \r\na");
            var stream = new StubNetworkStream(data);
            var chunkHandler = new ChunkedEncoding(stream, stream);

            //When
            chunkHandler.HandleChunked(bodyPart);

            //Then
            Assert.Equal("ab1234123", Encoding.UTF8.GetString(stream.GetWrittenBytes));
        }

        [Fact]
        public void Test_ChunkHandling_FirstBytesRead_is_NOT_Complete_Second_Test()
        {
            //Given
            const string data = "ab\r\n4\r\n1234\r\n 3\r\n123\r\n0\r\n\r\n";
            byte[] bodyPart = Encoding.UTF8.GetBytes("2 \r\n");
            var stream = new StubNetworkStream(data);
            var chunkHandler = new ChunkedEncoding(stream, stream);

            //When
            chunkHandler.HandleChunked(bodyPart);

            //Then
            Assert.Equal("ab1234123", Encoding.UTF8.GetString(stream.GetWrittenBytes));
        }

        [Fact]
        public void Test_ChunkHandling_FirstReadLine_is_NOT_Complete()
        {
            //Given
            const string data = "\nab\r\n4\r\n1234\r\n 3\r\n123\r\n0\r\n\r\n";
            byte[] bodyPart = Encoding.UTF8.GetBytes("2\r");
            var stream = new StubNetworkStream(data);
            var chunkHandler = new ChunkedEncoding(stream, stream);

            //When
            chunkHandler.HandleChunked(bodyPart);

            //Then
            Assert.Equal("ab1234123", Encoding.UTF8.GetString(stream.GetWrittenBytes));
        }

        [Fact]
        public void Test_ChunkHandling_More_AfterHeaders_Follow_ZeroSize_Chunk()
        {
            //Given
            const string data = "\nab\r\n 0\r\nHeader1\r\nHeader2\r\n\r\n";
            byte[] bodyPart = Encoding.UTF8.GetBytes("2\r");
            var stream = new StubNetworkStream(data);
            var chunkHandler = new ChunkedEncoding(stream, stream);

            //When
            chunkHandler.HandleChunked(bodyPart);

            //Then
            Assert.Equal("abHeader1\r\nHeader2\r\n\r\n",
                    Encoding.UTF8.GetString(stream.GetWrittenBytes));
        }

        [Fact]
        public void Test_ChunkHandling_NoBodyPart_Assure_ChunkSize_is_read_as_HEXA()
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
        public void Test_ChunkHandling_NoBodyPart_RepetiveProcess_For_More_Chunks()
        {
            //Given
            const string data = "2 \r\nab\r\n4\r\n1234\r\n 3\r\n123\r\n0\r\n\r\n";
            var stream = new StubNetworkStream(data);
            var chunkHandler = new ChunkedEncoding(stream, stream);

            //When
            chunkHandler.HandleChunked();

            //Then
            Assert.Equal("ab1234123", Encoding.UTF8.GetString(stream.GetWrittenBytes));
        }

        [Fact]
        public void Test_ChunkHandling_NoBodyPart_RepetiveProcess_For_Two_Chunks()
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

        [Fact]
        public void Test_ChunkHandling_SentBytes_For_One_Chunk_With_BodyPart()
        {
            //Given
            const string data = "0\r\n\r\n1234";
            var stream = new StubNetworkStream(DummyData);
            var chunkHandler = new ChunkedEncoding(stream, stream);

            //When
            chunkHandler.HandleChunked(Encoding.UTF8.GetBytes(data));

            //Then
            Assert.Null(stream.GetWrittenBytes);
        }

        [Fact]
        public void Test_ChunkHandling_SentBytes_For_Two_Chunks_With_BodyPart()
        {
            //Given
            const string data = "4\r\n1234\r\n0\r\n\r\n1234";
            var stream = new StubNetworkStream(data);
            var chunkHandler = new ChunkedEncoding(stream, stream);

            //When
            chunkHandler.HandleChunked(Encoding.UTF8.GetBytes(data));

            //Then
            Assert.Equal("1234", Encoding.UTF8.GetString(stream.GetWrittenBytes));
        }

        [Fact]
        public void Test_ChunkHandling_With_BodyPart_After_Reading_Headers()
        {
            //Given
            const string data = "4\r\n1234\r\n 3\r\n123\r\n0\r\n\r\n";
            byte[] bodyPart = Encoding.UTF8.GetBytes("2 \r\nab\r\n");
            var stream = new StubNetworkStream(data);
            var chunkHandler = new ChunkedEncoding(stream, stream);

            //When
            chunkHandler.HandleChunked(bodyPart);

            //Then
            Assert.Equal("ab1234123", Encoding.UTF8.GetString(stream.GetWrittenBytes));
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
        public void Test_ReadSendBytes_Written_Bytes_for_Simple_case()
        {
            // Given
            byte[] bodyPart = Encoding.UTF8.GetBytes(TenBytes);
            var stream = new StubNetworkStream(DummyData);
            var chunkHandler = new ChunkedEncoding(stream, stream);

            // When
            chunkHandler.ReadAndSendChunk(bodyPart, 5);

            // Then
            Assert.Equal("12345", Encoding.UTF8.GetString(stream.GetWrittenBytes));
        }

        [Fact]
        public void Test_Remainder_After_Reading_MoreBytes_Then_BodyPart_length()
        {
            // Given
            byte[] bodyPart = Encoding.UTF8.GetBytes(TenBytes);
            var stream = new StubNetworkStream("ABC");
            var chunkHandler = new ChunkedEncoding(stream, stream);

            // When
            chunkHandler.ReadAndSendChunk(bodyPart, 12);

            // Then
            Assert.Null(chunkHandler.Remainder);
        }

        [Fact]
        public void Test_Remainder_After_ReadingBytes_SimpleCase()
        {
            // Given
            byte[] bodyPart = Encoding.UTF8.GetBytes(TenBytes);
            var stream = new StubNetworkStream(DummyData);
            var chunkHandler = new ChunkedEncoding(stream, stream);

            // When
            chunkHandler.ReadAndSendChunk(bodyPart, 5);

            // Then
            Assert.Equal("67890", Encoding.UTF8.GetString(chunkHandler.Remainder));
        }

        [Fact]
        public void Test_Remainder_Should_be_NULL_NO_StreamRead_Occured()
        {
            // Given
            var stream = new StubNetworkStream(DummyData);

            // When
            var chunkHandler = new ChunkedEncoding(stream, stream);

            // Then
            Assert.Null(chunkHandler.Remainder);
        }

        [Fact]
        public void Test_WrittenBytes_After_Reading_MoreBytes_Then_BodyPart_length()
        {
            // Given
            byte[] bodyPart = Encoding.UTF8.GetBytes(TenBytes);
            var stream = new StubNetworkStream("ABCD");
            var chunkHandler = new ChunkedEncoding(stream, stream);

            // When
            chunkHandler.ReadAndSendChunk(bodyPart, 13);

            // Then
            Assert.Equal("1234567890ABC", Encoding.UTF8.GetString(stream.GetWrittenBytes));
        }
    }
}