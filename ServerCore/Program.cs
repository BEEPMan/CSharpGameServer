using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            // DNS (Domain Name System) & IP Endpoint
            IPHostEntry iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress iPAddress = iPHostEntry.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(iPAddress, 7777);

            try
            {
                _listener.Init(localEndPoint);

                // Accept
                while (true)
                {
                    Console.WriteLine("Listening...");

                    Socket clientSocket = _listener.Accept();

                    // Receive message
                    byte[] recvBuffer = new byte[1024];
                    int recvBytes = clientSocket.Receive(recvBuffer);
                    string recvData = Encoding.UTF8.GetString(recvBuffer, 0, recvBytes);
                    Console.WriteLine($"[From Client] {recvData}");

                    // Send message
                    byte[] sendBuffer = Encoding.UTF8.GetBytes("Welcome to Game Server!");
                    clientSocket.Send(sendBuffer);

                    // Disconnect
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}