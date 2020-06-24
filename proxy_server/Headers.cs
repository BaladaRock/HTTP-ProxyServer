﻿using System.Text;

namespace ProxyServer
{
    internal static class Headers
    {
        internal const string EmptyLine = "\r\n\r\n";
        internal const string NewLine = "\r\n";
        internal const string ZeroChunk = "0\r\n";

        internal static byte[] Chunked => GetBytes("chunked");

        internal static byte[] ChunkedHeader => GetBytes("transfer-encoding: ");

        internal static byte[] ContentHeader => GetBytes("content-length: ");

        internal static byte[] EmptyLineBytes => GetBytes(EmptyLine);

        internal static byte[] NewLineByte => GetBytes(NewLine);

        private static byte[] GetBytes(string toConvert)
        {
            return Encoding.UTF8.GetBytes(toConvert);
        }
    }
}