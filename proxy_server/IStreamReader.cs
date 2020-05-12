namespace ProxyServer
{
    public interface IStreamReader
    {
        int Read(byte[] buffer, int offset, int size);
    }
}