using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpClient
{
    class Client
    {
        static void Main(string[] args)
        {
            try
            {
                System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient("localhost", 8080);
                byte[] request = Encoding.UTF8.GetBytes("GET / HTTP/1.0\r\n\r\n");


                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(request, 0, request.Length);

                Console.WriteLine("Send: + Encoding.ASCII.GetString(request)");

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                byte[] data = new byte[256];

                // String to store the response ASCII representation.
                string responseData = string.Empty;

                // Read the first batch of the TcpServer response bytes.
                int bytes = stream.Read(data, 0, data.Length);
                responseData = Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.Read();
        }
    }
}
