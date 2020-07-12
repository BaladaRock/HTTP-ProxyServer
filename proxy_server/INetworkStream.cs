namespace ProxyServer
{
    public interface INetworkStream
    {
        bool DataAvailable { get; }

        int Read(byte[] buffer, int offset, int size);

        void Write(byte[] buffer, int offset, int size);
    }
}