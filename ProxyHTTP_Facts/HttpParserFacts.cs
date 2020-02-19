using ProxyHTTP;
using Xunit;

namespace ProxyHTTP_Facts
{
    public class HttpParserFacts
    {
        [Fact]
        public void Should_Know_If_A_Response_Contains_EMPTY_LINE()
        {
            //Given
            byte[] response = new byte[]
            {
                (byte)'a', (byte)'b',
                (byte)'c', (byte)'d',
                (byte)'\r', (byte)'\n',
                (byte)'\r', (byte)'\n',
                (byte)'a', (byte)'b',
            };

            //When
            var parser = new HttpParser(response);
            byte[] subArray = new byte[]
            {
                (byte)'\r', (byte)'\n',
                (byte)'\r', (byte)'\n'}
            ;

            //Then
            Assert.True(parser.Contains(subArray));
        }

        [Fact]
        public void Should_Return_FALSE_Array_DoesNot_Contain_SubArray()
        {
            //Given
            byte[] response = new byte[]
            {
                (byte)'a', (byte)'b',
                (byte)'c', (byte)'d',
                (byte)'\r', (byte)'\n',
                (byte)'\r', (byte)'n',
                (byte)'a', (byte)'b',
            };

            //When
            var parser = new HttpParser(response);
            byte[] subArray = new byte[] { (byte)'\r', (byte)'\r' };

            //Then
            Assert.False(parser.Contains(subArray));
        }

        [Fact]
        public void Should_Return_TRUE_SubArray_is_at_THE_END()
        {
            //Given
            byte[] response = new byte[]
            {
                (byte)'a', (byte)'c',
                (byte)'c', (byte)'d',
                (byte)'\r', (byte)'\n',
                (byte)'\r', (byte)'\n',
                (byte)'a', (byte)'b',
            };

            //When
            var parser = new HttpParser(response);
            byte[] subArray = new byte[] { (byte)'\r', (byte)'\n', (byte)'a', (byte)'b' };

            //Then
            Assert.True(parser.Contains(subArray));
            Assert.False(parser.Contains(new byte[] { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n'} ));

        }
    }
}