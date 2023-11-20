using ServerCore;
using System.Collections.Concurrent;

namespace DummyClient
{
    public class SessionManager
    {
        private static SessionManager _instance;

        private SessionManager() { }

        public static SessionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SessionManager();
                }
                return _instance;
            }
        }

        public ConcurrentDictionary<int, Player> Players = new ConcurrentDictionary<int, Player>();

        public StreamWriter sessionLog;

        private List<ServerSession> _sessions = new List<ServerSession>();
        object _lock = new object();

        public void SendMove()
        {
            lock (_lock)
            {
                int count = 0;
                foreach (ServerSession session in _sessions)
                {
                    if (session.SessionId == 0 || !Players.ContainsKey(session.SessionId))
                    {
                        continue;
                    }
                    C_MOVE packet = new C_MOVE
                    {
                        PosX = Players[session.SessionId].posX,
                        PosY = Players[session.SessionId].posY,
                        PosZ = Players[session.SessionId].posZ,
                        VelX = Players[session.SessionId].velX,
                        VelY = Players[session.SessionId].velY,
                        VelZ = Players[session.SessionId].velZ,
                        TimeStamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds
                    };
                    byte[] sendBuffer = Utils.SerializePacket(PacketType.PKT_C_MOVE, packet);
                    session.Send(sendBuffer);
                    count++;
                    sessionLog.WriteLine($"session {session.SessionId} send C_Move");
                }
                sessionLog.WriteLine($"============== {count} / {_sessions.Count} sessions ==============");
            }
        }

        public void SimulateMove(float speed)
        {
            Console.WriteLine(DateTime.Now.Second + (float) DateTime.Now.Millisecond / 1000);
            float prevTime = 0.0f;
            DateTime start = DateTime.Now;
            while(Program.moveEvents.Count > 0)
            {
                Program.MoveEvent moveEvent = Program.moveEvents.Dequeue();
                if (Players.TryGetValue(moveEvent.playerId, out _) == false)
                {
                    continue;
                }
                if (moveEvent.startTime - prevTime > 0)
                    Thread.Sleep((int)((moveEvent.startTime - prevTime) * 1000));
                prevTime = (float)((DateTime.Now - start).TotalMilliseconds / 1000);
                Players[moveEvent.playerId].SetVelocity(moveEvent.velX * speed, moveEvent.velY * speed, moveEvent.velZ * speed);
                Task.Run(() => Players[moveEvent.playerId].Move(moveEvent.time));
            }
            Console.WriteLine(DateTime.Now.Second + (float) DateTime.Now.Millisecond / 1000);
            Console.WriteLine("=============== Simulation Result ===============");
            for(int i=1;i<=Program.DUMMY_COUNT;i++)
            {
                Console.WriteLine($"Player {i} : {Players[i].posX}, {Players[i].posY}, {Players[i].posZ}");
            }
            Console.WriteLine("=================================================");
        }

        public void DisconnectAll()
        {
            foreach (Session session in _sessions)
            {
                session.Disconnect();
            }
        }

        public ServerSession AddSession()
        {
            lock (_lock)
            {
                ServerSession session = new ServerSession();
                _sessions.Add(session);
                sessionLog.WriteLine($"session added");

                return session;
            }
        }

        public void RemoveSession(ServerSession session)
        {
            lock (_lock)
                _sessions.Remove(session);
        }
    }
}