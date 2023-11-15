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
        public static int DUMMY_COUNT = 9;

        public static PriorityQueue<MoveEvent, float> moveEvents = new PriorityQueue<MoveEvent, float>();

        // public static Dictionary<int, List<MoveEvent>> moveEvents = new Dictionary<int, List<MoveEvent>>();

        static void Main(string[] args)
        {
            // AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnExit);

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Thread.Sleep(1000);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return SessionManager.Instance.AddSession(); }, DUMMY_COUNT);

            CreateMoveEvents();

            DateTime now = DateTime.Now;
            DateTime scheduledTime = new DateTime(now.Year, now.Month, now.Day, 17, 28, 30);

            if(now > scheduledTime)
            {
                scheduledTime = now;
            }

            double initialDelay = (scheduledTime - now).TotalMilliseconds;

            Timer timer = new Timer(MoveWork, null, (int)initialDelay, Timeout.Infinite);

            Thread.Sleep((int)initialDelay);

            while (true)
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

        public static void MoveWork(object state)
        {
            //while (Console.ReadKey().Key != ConsoleKey.Q)
            //{
            //    SessionManager.Instance.MoveForEach(5.0f);
            //    Thread.Sleep(250);
            //}

            // Thread.Sleep(5000);

            SessionManager.Instance.SimulateMove(5.0f);

            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
                Thread.Sleep(100);
            }

            SessionManager.Instance.DisconnectAll();
            Console.WriteLine("Disconnected all sessions.");
        }

        public struct MoveEvent
        {
            public float startTime;
            public float time;

            public int playerId;

            public float velX;
            public float velY;
            public float velZ;
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

            List<MoveEvent> playerMoveEvents = new List<MoveEvent>();

            sr.ReadLine();
            while(sr.EndOfStream == false)
            {
                string line = sr.ReadLine();
                string[] split = line.Split(',');

                float time = float.Parse(split[0]);
                float startTime = float.Parse(split[1]);

                if (time + startTime >= 30.0f)
                {
                    moveEvents.Enqueue(new MoveEvent()
                    {
                        startTime = startTime,
                        time = 30.0f - startTime,
                        playerId = playerId,
                        velX = float.Parse(split[3]),
                        velY = 0,
                        velZ = float.Parse(split[4])
                    }, startTime);
                    moveEvents.Enqueue(new MoveEvent()
                    {
                        startTime = 30.0f,
                        time = 10.0f,
                        playerId = playerId,
                        velX = 0,
                        velY = 0,
                        velZ = 0
                    }, 30.0f);
                    break;
                }

                moveEvents.Enqueue(new MoveEvent()
                {
                    startTime = startTime,
                    time = time,
                    playerId = playerId,
                    velX = float.Parse(split[3]),
                    velY = 0,
                    velZ = float.Parse(split[4])
                }, startTime);
            }
        }

        //static void OnExit(object sender, EventArgs e)
        //{
        //    SessionManager.Instance.DisconnectAll();
        //}
    }
}