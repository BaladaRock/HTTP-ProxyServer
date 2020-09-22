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

        public static byte[] FailureStatusMessage => GetBytes("503 Service Unavailable");
        internal static byte[] NewLineByte => GetBytes(NewLine);
        internal static byte[] SuccesStatusMessage => GetBytes("HTTP/1.1 200 OK");

        private static byte[] GetBytes(string toConvert)
        {
            return Encoding.UTF8.GetBytes(toConvert);
        }
    }
}