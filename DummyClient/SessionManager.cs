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
            foreach (ServerSession session in _sessions)
            {
                C_CHAT packet = new C_CHAT { Chat = data };
                byte[] sendBuffer = Utils.SerializePacket(PacketType.PKT_C_CHAT, packet);
                session.Send(sendBuffer);
            }
        }

        public bool CheckLogin()
        {
            if(_sessions.Count == 0)
                return false;
            foreach (ServerSession session in _sessions)
            {
                if(session.isLogin == true)
                    return true;
            }
            return false;
        }

        public ServerSession AddSession()
        {
            ServerSession session = new ServerSession();
            _sessions.Add(session);

            Console.WriteLine($"New session connected.");

            return session;
        }

        public void RemoveSession(ServerSession session)
        {
            _sessions.Remove(session);
        }
    }
}