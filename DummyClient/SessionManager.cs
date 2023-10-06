using ServerCore;

namespace DummyClient
{
    public class SessionManager
    {
        private static SessionManager _instance;
        private List<ServerSession> sessions = new List<ServerSession>();

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
            sessions.Add(session);
            return session;
        }

        public void RemoveSession(ServerSession session)
        {
            sessions.Remove(session);
        }
    }
}