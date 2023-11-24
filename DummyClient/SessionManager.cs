using ServerCore;
using System.Collections.Concurrent;
using System.Diagnostics;

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
                        TimeStamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds
                    };
                    ArraySegment<byte> sendBuffer = Utils.SerializePacket(PacketType.PKT_C_MOVE, packet);
                    session.Send(sendBuffer);
                    count++;
                    sessionLog.WriteLine($"session {session.SessionId} send C_Move");
                }
                sessionLog.WriteLine($"============== {count} / {_sessions.Count} sessions ==============");
            }
        }

        public void SendMove_v2()
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
                    C_MOVE_V2 packet = new C_MOVE_V2
                    {
                        PosX = Players[session.SessionId].posX,
                        PosY = Players[session.SessionId].posY,
                        PosZ = Players[session.SessionId].posZ,
                        VelX = Players[session.SessionId].velX,
                        VelY = Players[session.SessionId].velY,
                        VelZ = Players[session.SessionId].velZ,
                        TimeStamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds
                    };
                    ArraySegment<byte> sendBuffer = Utils.SerializePacket(PacketType.PKT_C_MOVE_V2, packet);
                    session.Send(sendBuffer);
                    count++;
                    sessionLog.WriteLine($"session {session.SessionId} send C_Move_V2");
                }
                sessionLog.WriteLine($"============== {count} / {_sessions.Count} sessions ==============");
            }
        }

        public void SendMove_v3()
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
                    float speed = MathF.Sqrt(Players[session.SessionId].velX * Players[session.SessionId].velX + Players[session.SessionId].velZ * Players[session.SessionId].velZ);
                    float theta = 0.0f;
                    if (Players[session.SessionId].velX != 0)
                    {
                        theta = MathF.Atan2(Players[session.SessionId].velZ / speed, Players[session.SessionId].velX / speed) / (2 * MathF.PI);
                    }
                    C_MOVE_V3 packet = new C_MOVE_V3
                    {
                        Theta = theta,
                        Speed = speed,
                    };
                    ArraySegment<byte> sendBuffer = Utils.SerializePacket(PacketType.PKT_C_MOVE_V3, packet);
                    session.Send(sendBuffer);
                    count++;
                    sessionLog.WriteLine($"session {session.SessionId} send C_Move_V3");
                }
                sessionLog.WriteLine($"============== {count} / {_sessions.Count} sessions ==============");
            }
        }

        public void SendPos()
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
                    C_POS packet = new C_POS
                    {
                        PosX = Players[session.SessionId].posX,
                        PosY = Players[session.SessionId].posY,
                        PosZ = Players[session.SessionId].posZ,
                        VelX = Players[session.SessionId].velX,
                        VelY = Players[session.SessionId].velY,
                        VelZ = Players[session.SessionId].velZ,
                        TimeStamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds
                    };
                    ArraySegment<byte> sendBuffer = Utils.SerializePacket(PacketType.PKT_C_POS, packet);
                    session.Send(sendBuffer);
                    count++;
                    sessionLog.WriteLine($"session {session.SessionId} send C_Pos");
                }
                sessionLog.WriteLine($"============== {count} / {_sessions.Count} sessions ==============");
            }
        }

        private Stopwatch _stopwatch = new Stopwatch();
        private float _totalRunTime = 0.0f;
        private float _speed = 10.0f;

        public void SimulateMove()
        {
            _stopwatch.Start();

            while(_totalRunTime <= 30.0f)
            {
                Update();
                Thread.Sleep(10);
            }
            Console.WriteLine("=============== Simulation Result ===============");
            for (int i = 1; i <= Program.DUMMY_COUNT; i++)
            {
                Console.WriteLine($"Player {i} : {Players[i].posX}, {Players[i].posY}, {Players[i].posZ}");
            }
            Console.WriteLine("=================================================");
        }

        public void Update()
        {
            float deltaTime = (float)(_stopwatch.Elapsed.TotalSeconds);
            _totalRunTime += deltaTime;
            _stopwatch.Restart();

            foreach (KeyValuePair<int, Player> player in Players)
            {
                if (Program.moveEvents[player.Key].Count > 0)
                {
                    if (Program.moveEvents[player.Key].Peek().startTime <= _totalRunTime)
                    {
                        Program.MoveEvent moveEvent = Program.moveEvents[player.Key].Dequeue();
                        player.Value.SetVelocity(moveEvent.velX * _speed, moveEvent.velY * _speed, moveEvent.velZ * _speed);
                    }
                }
                player.Value.Move(deltaTime);
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