using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static Listener _listener = new Listener();

        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                Session session = new Session();
                session.Init(clientSocket);

                // Send message
                byte[] sendBuffer = Encoding.UTF8.GetBytes("Welcome to Game Server!");
                session.Send(sendBuffer);

                Thread.Sleep(1000);

                session.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            // DNS (Domain Name System) & IP Endpoint
            IPHostEntry iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress iPAddress = iPHostEntry.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(iPAddress, 7777);

            _listener.Init(localEndPoint, OnAcceptHandler);
            Console.WriteLine("Listening...");

            while (true)
            {
                
            }
        }
    }
}