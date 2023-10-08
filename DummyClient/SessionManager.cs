using ServerCore;

namespace DummyClient
{
    public class SessionManager
    {
        private static SessionManager _instance;
        private List<ServerSession> _sessions = new List<ServerSession>();

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