using ProxyHTTP;
using System.Text;
using Xunit;

namespace ProxyHTTP_Facts
{
    public class HttpParserFacts
    {
        [Fact]
        public void Should_Correctly_Give_Position_Array_Has_More_SubArrays()
        {
            // Given
            const string data = "bcbcababab";

            // When
            var parser = new HttpParser(Encoding.UTF8.GetBytes(data));
            const string subArray = "ab";
            int position = parser.GetPosition(Encoding.UTF8.GetBytes(subArray));

            // Then
            Assert.Equal(6, position);
        }

        [Fact]
        public void Should_Correctly_Give_Position_Final_Test()
        {
            // Given
            const string data = "abbcababab";

            // When
            var parser = new HttpParser(Encoding.UTF8.GetBytes(data));
            const string subArray = "ab";
            int position = parser.GetPosition(Encoding.UTF8.GetBytes(subArray));

            //Then
            Assert.Equal(2, position);
        }

        [Fact]
        public void Should_Correctly_Give_Position_Of_EMPTY_LINE()
        {
            // Given
            const string data = "abcd\r\n\r\nab";

            // When
            var parser = new HttpParser(Encoding.UTF8.GetBytes(data));
            const string subArray = "\r\n\r\n";
            int position = parser.GetPosition(Encoding.UTF8.GetBytes(subArray));

            // Then
            Assert.Equal(8, position);
        }

        [Fact]
        public void Test_Give_Position_Should_Take_the_ENTIRE_SUBARRAY()
        {
            // Given
            const string data = "abefabcd";

            // When
            var parser = new HttpParser(Encoding.UTF8.GetBytes(data));
            const string subArray = "abcd";
            int position = parser.GetPosition(Encoding.UTF8.GetBytes(subArray));

            // Then
            Assert.Equal(8, position);
        }

        [Fact]
        public void Should_Know_If_A_Response_Contains_EMPTY_LINE()
        {
            // Given
            const string data = "abcd\r\n\r\nab";

            // When
            var parser = new HttpParser(Encoding.UTF8.GetBytes(data));
            const string subArray = "\r\n\r\n";

            // Then
            Assert.True(parser.Contains(Encoding.UTF8.GetBytes(subArray)));
        }

        [Fact]
        public void Should_Return_FALSE_Array_DoesNot_Contain_SubArray()
        {
            // Given
            const string data = "abcd\r\n\r\nab";

            // When
            var parser = new HttpParser(Encoding.UTF8.GetBytes(data));
            const string subArray = "\r\r";

            // Then
            Assert.False(parser.Contains(Encoding.UTF8.GetBytes(subArray)));
        }

        [Fact]
        public void Should_Return_TRUE_SubArray_is_at_THE_END()
        {
            // Given
            const string data = "abcd\r\n\r\nab";

            // When
            var parser = new HttpParser(Encoding.UTF8.GetBytes(data));
            const string subArray = "\r\nab";

            // Then
            Assert.True(parser.Contains(Encoding.UTF8.GetBytes(subArray)));
        }
    }
}