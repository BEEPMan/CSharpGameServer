using ServerCore;

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

        public void MoveForEach(float speed)
        {
            lock (_lock)
            {
                Random rand = new Random();
                foreach (ServerSession session in _sessions)
                {
                    if(session.SessionId == 0)
                    {
                        continue;
                    }
                    float x = (float)rand.NextDouble() * speed * 2 - speed;
                    float z;
                    if (rand.Next(0, 2) == 0)
                        z = MathF.Sqrt(speed * speed - x * x);
                    else
                        z = -1 * MathF.Sqrt(speed * speed - x * x);
                    float posX = session.Players[session.SessionId].posX + x;
                    float posY = session.Players[session.SessionId].posY;
                    float posZ = session.Players[session.SessionId].posZ + z;
                    session.Players[session.SessionId] = new Player { posX = posX, posY = posY, posZ = posZ };
                    C_MOVE packet = new C_MOVE { PosX = posX, PosY = posY, PosZ = posZ };
                    byte[] sendBuffer = Utils.SerializePacket(PacketType.PKT_C_MOVE, packet);
                    session.Send(sendBuffer);
                }
            }
        }

        public ServerSession AddSession()
        {
            lock (_lock)
            {
                ServerSession session = new ServerSession();
                // TODO : Add가 여러번 호출되고 있는 상태를 해결해야 함
                _sessions.Add(session);

                Console.WriteLine($"New session connected.");

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