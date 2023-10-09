﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return SessionManager.Instance.AddSession(); }, 5);
            
            while(true)
            {
                try
                {
                    // string chat = Console.ReadLine();
                    SessionManager.Instance.SendForEach("Hello World!");
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                    break;
                }

                Thread.Sleep(250);
            }
        }
    }
}