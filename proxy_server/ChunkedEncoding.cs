﻿using ProxyServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProxyHTTP
{
    internal class ChunkedEncoding
    {
        private INetworkStream serverStream;
        private INetworkStream browserStream;
        private byte[] bodyPart;

        public ChunkedEncoding(
            INetworkStream serverStream,
            INetworkStream browserStream,
            byte[] bodyPart = null)
        {
            this.serverStream = serverStream;
            this.browserStream = browserStream;
            this.bodyPart = bodyPart;
        }

        public void ReadAndSendBytes(string toRead)
        {

            var bytesReader = new ContentLength(serverStream, browserStream);
            bytesReader.HandleResponseBody(bodyPart, toRead);
        }

        internal int ConvertFromHexadecimal(string hexa)
        {
            return Convert.ToInt32(hexa.Trim(), 16);
        }
    }
}