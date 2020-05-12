using ProxyServer;

namespace ProxyHTTP_Facts
{
    internal class StubNetworkStream : IStreamReader
    {
        private string data;

        public StubNetworkStream(string data)
        {
            this.data = data;

        }

        public int Read(byte[] buffer, int offset, int size)
        {
            throw new System.NotImplementedException();
        }
    }
}