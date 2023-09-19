
namespace ServerCore
{
    public class SessionManager
    {
        private static SessionManager _instance;
        private List<Session> sessions = new List<Session>();

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

        public void AddSession(Session session)
        {
            sessions.Add(session);
        }

        public void RemoveSession(Session session)
        {
            sessions.Remove(session);
        }

        public int GetSessionIndex(Session session)
        {
            return sessions.IndexOf(session);
        }

        public void BroadCast(string data)
        {
            foreach (Session session in sessions)
            {
                session.SendDataAsync(data);
            }
        }

        // 다른 관리 기능들
    }
}