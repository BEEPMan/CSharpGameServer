using System;
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
            // AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnExit);

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Thread.Sleep(1000);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return SessionManager.Instance.AddSession(); }, 9);

            Thread thread = new Thread(new ThreadStart(MoveWork));
            thread.Start();

            while(true)
            {
                try
                {
                    // SessionManager.Instance.SendForEach("Hello World!");
                    SessionManager.Instance.SendMove();

                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                    break;
                }
                Thread.Sleep(250);
            }

            thread.Join();
            // SessionManager.Instance.DisconnectAll();
        }

        public static void KeyEventWork()
        {
            while(Console.ReadKey().Key != ConsoleKey.Q)
            {
                Thread.Sleep(100);
            }

            SessionManager.Instance.DisconnectAll();
            Console.WriteLine("Disconnected all sessions.");
        }

        public static void MoveWork()
        {
            while(Console.ReadKey().Key != ConsoleKey.Q)
            {
                SessionManager.Instance.MoveForEach(5.0f);
                Thread.Sleep(250);
            }

            SessionManager.Instance.DisconnectAll();
            Console.WriteLine("Disconnected all sessions.");
        }

        //static void OnExit(object sender, EventArgs e)
        //{
        //    SessionManager.Instance.DisconnectAll();
        //}
    }
}