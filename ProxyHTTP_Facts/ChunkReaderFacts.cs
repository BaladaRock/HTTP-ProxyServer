using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ProxyHTTP_Facts
{
    public class ChunkReaderFacts
    {
        [Fact]
        public void Test_LineReader_Should_Read_LeadingLine_for_given_CHUNK()
        {
            //Given
            byte[] writeOnStream = new byte[]
            {
                (byte)'3', (byte)'\r',
                (byte)'\n', (byte)'a',
                (byte)'b', (byte)'c',
                (byte)'\r', (byte)'\n',
                (byte)'2', (byte)'b',
            };

            MemoryStream stream = new MemoryStream(writeOnStream);
            var chunkReader = new ChunkReader(stream);

            //When
            string size = chunkReader.GetChunkSize();

            //Then
            Assert.Equal("3", size);
        }

        [Fact]
        public void Test_LineReader_Should_Read_Bytes_for_given_chunk_SIZE()
        {
            //Given
            byte[] writeOnStream = new byte[]
            {
                (byte)'3', (byte)'\r',
                (byte)'\n', (byte)'a',
                (byte)'b', (byte)'c',
                (byte)'\r', (byte)'\n',
                (byte)'2', (byte)'b',
            };

            MemoryStream stream = new MemoryStream(writeOnStream);
            byte[] toCheck = new byte[] { (byte)'a', (byte)'b', (byte)'c' };
            var chunkReader = new ChunkReader(stream);

            //When
            string line = chunkReader.GetChunkSize();
            byte[] byteLine = chunkReader.ReadBytes(line);

            //Then
            Assert.True(byteLine.SequenceEqual(toCheck));
        }
    }
}
