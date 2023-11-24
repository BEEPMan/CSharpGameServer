using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace DummyClient
{
    class Program
    {
        public struct MoveEvent
        {
            public float startTime;
            public float time;

            public int playerId;

            public float velX;
            public float velY;
            public float velZ;
        }

        public static int DUMMY_COUNT = 99;

        public static Dictionary<int, Queue<MoveEvent>> moveEvents = new Dictionary<int, Queue<MoveEvent>>();

        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + $"\\..\\..\\Logs/";
            SessionManager.Instance.sessionLog = new StreamWriter(path + "sessionLog.txt");

            Thread.Sleep(1000);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return SessionManager.Instance.AddSession(); }, DUMMY_COUNT);

            CreateMoveEvents();

            while (Console.ReadKey().Key != ConsoleKey.S)
            {
                Thread.Sleep(100);
            }

            Thread thread = new Thread(MoveWork);
            thread.Start();

            int count = 0;
            while (true)
            {
                try
                {
                    ////////// SendMove //////////
                    //SessionManager.Instance.SendMove();
                    //////////////////////////////
                    ////////// SendMove_v2 //////////
                    //SessionManager.Instance.SendMove_v2();
                    //////////////////////////////
                    ////////// SendMove_v3 //////////
                    count++;
                    if (count == 4)
                    {
                        SessionManager.Instance.SendPos();
                        count = 0;
                    }
                    else
                    {
                        SessionManager.Instance.SendMove_v3();
                    }
                    /////////////////////////////////
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    break;
                }
                Thread.Sleep(250);
            }

            thread.Join();
        }

        public static void MoveWork()
        {
            SessionManager.Instance.SimulateMove();

            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
                Thread.Sleep(100);
            }

            SessionManager.Instance.sessionLog.Close();
            SessionManager.Instance.DisconnectAll();
            Console.WriteLine("Disconnected all sessions.");
        }

        public static void CreateMoveEvents()
        {
            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + $"\\..\\..\\Data/";

            for (int i=1;i<=DUMMY_COUNT;i++)
            {
                ReadTimeline(path + $"Player_{i}.csv", i);
            }
        }

        public static void ReadTimeline(string path, int playerId)
        {
            StreamReader sr = new StreamReader(path);

            moveEvents.Add(playerId, new Queue<MoveEvent>());

            sr.ReadLine();
            while(sr.EndOfStream == false)
            {
                string line = sr.ReadLine();
                string[] split = line.Split(',');

                float time = float.Parse(split[0]);
                float startTime = float.Parse(split[1]);

                moveEvents[playerId].Enqueue(new MoveEvent()
                {
                    startTime = startTime,
                    time = time,
                    playerId = playerId,
                    velX = float.Parse(split[3]),
                    velY = 0,
                    velZ = float.Parse(split[4])
                });
            }
        }
    }
}