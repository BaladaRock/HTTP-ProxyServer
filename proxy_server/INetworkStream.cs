namespace ProxyServer
{
    public interface INetworkStream
    {
        int Read(byte[] buffer, int offset, int size);

        void Write(byte[] buffer, int offset, int size);
    }
}