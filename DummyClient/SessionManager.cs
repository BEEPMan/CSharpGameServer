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

        private List<ServerSession> _sessions = new List<ServerSession>();
        object _lock = new object();

        public void SendForEach(string data)
        {
            lock (_lock)
            {
                foreach (ServerSession session in _sessions)
                {
                    C_CHAT packet = new C_CHAT { Chat = data };
                    byte[] sendBuffer = Utils.SerializePacket(PacketType.PKT_C_CHAT, packet);
                    session.Send(sendBuffer);
                }
            }
        }

        public void SendMove()
        {
            lock (_lock)
            {
                foreach (ServerSession session in _sessions)
                {
                    if (session.SessionId == 0 || !Players.ContainsKey(session.SessionId))
                    {
                        continue;
                    }
                    C_MOVE packet = new C_MOVE { PosX = Players[session.SessionId].posX,
                                                 PosY = Players[session.SessionId].posY,
                                                 PosZ = Players[session.SessionId].posZ,
                                                 VelX = Players[session.SessionId].velX,
                                                 VelY = Players[session.SessionId].velY,
                                                 VelZ = Players[session.SessionId].velZ,
                    };
                    byte[] sendBuffer = Utils.SerializePacket(PacketType.PKT_C_MOVE, packet);
                    session.Send(sendBuffer);
                }
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
        }

        public void MoveForEach(float speed)
        {
            Random rand = new Random();
            foreach (ServerSession session in _sessions)
            {
                if(session.SessionId == 0)
                {
                    continue;
                }
                if (Players.TryGetValue(session.SessionId, out _) == false)
                {
                    continue;
                }
                float x = (float)rand.NextDouble() * speed * 2 - speed;
                float z;
                if (rand.Next(0, 2) == 0)
                    z = MathF.Sqrt(speed * speed - x * x);
                else
                    z = -1 * MathF.Sqrt(speed * speed - x * x);
                Players[session.SessionId].SetVelocity(x, 0, z);
                Players[session.SessionId].Move(0.25f);
            }
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
                // TODO : Add가 여러번 호출되고 있는 상태를 해결해야 함
                _sessions.Add(session);

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