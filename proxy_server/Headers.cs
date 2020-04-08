using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("ProxyHTTP_Facts")]

namespace ProxyHTTP
{
    internal static class Headers
    {
        internal const string EmptyLine = "\r\n\r\n";
        internal const string NewLine = "\r\n";

        internal static byte[] Chunked => GetBytes("chunked");

        internal static byte[] ChunkedHeader => GetBytes("Transfer-Encoding: ");

        internal static byte[] ContentHeader => GetBytes("Content-Length: ");

        internal static byte[] EmptyLineByte => GetBytes(EmptyLine);

        private static byte[] GetBytes(string toConvert)
        {
            return Encoding.UTF8.GetBytes(toConvert);
        }
    }
}