using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // DNS (Domain Name System) & IP Endpoint
            IPHostEntry iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress iPAddress = iPHostEntry.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(iPAddress, 7777);

            // TCP Socket
            Socket socket = new(
                iPAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            try
            {
                // Connect
                socket.Connect(localEndPoint);
                Console.WriteLine($"Connected To {socket.RemoteEndPoint}");

                // Send message
                byte[] sendBuffer = Encoding.UTF8.GetBytes("Hello World!");
                int sendBytes = socket.Send(sendBuffer);

                // Receive message
                byte[] recvBuffer = new byte[1024];
                int recvBytes = socket.Receive(recvBuffer);
                string recvData = Encoding.UTF8.GetString(recvBuffer, 0, recvBytes);
                Console.WriteLine($"[From Server] {recvData}");

                // Disconnect
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}