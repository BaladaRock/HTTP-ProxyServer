using System.Text;

namespace ProxyServer
{
    internal static class Headers
    {
        internal const string Chunked = "chunked";

        internal const string ChunkedHeader = "transfer-encoding:";

        internal const string ContentHeader = "content-length:";

        internal const string EmptyLine = "\r\n\r\n";

        internal const string NewLine = "\r\n";

        internal static byte[] NewLineByte => GetBytes(NewLine);

        private static byte[] GetBytes(string toConvert)
        {
            return Encoding.UTF8.GetBytes(toConvert);
        }
    }
}