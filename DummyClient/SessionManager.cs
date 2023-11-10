﻿using ServerCore;

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

        public Dictionary<int, Player> Players = new Dictionary<int, Player>();

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
                    C_MOVE packet = new C_MOVE { PosX = Players[session.SessionId].posX, PosY = Players[session.SessionId].posY, PosZ = Players[session.SessionId].posZ };
                    byte[] sendBuffer = Utils.SerializePacket(PacketType.PKT_C_MOVE, packet);
                    session.Send(sendBuffer);
                }
            }
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
                if (Players.TryGetValue(session.SessionId, out Player player) == false)
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