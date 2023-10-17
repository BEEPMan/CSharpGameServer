﻿using ServerCore;

namespace GameServer
{
    public class SessionManager
    {
        private static SessionManager _instance;
        private Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();

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

        private int _sessionId = 0;

        public ClientSession AddSession()
        {
            ClientSession session = new ClientSession();
            session.SessionId = Interlocked.Increment(ref _sessionId);

            Console.WriteLine($"New session connected. SessionId: {session.SessionId}");

            _sessions.Add(session.SessionId, session);

            return session;
        }

        public void RemoveSession(ClientSession session)
        {
            _sessions.Remove(session.SessionId);
        }

        public void BroadCast(byte[] data)
        {
            foreach (int index in _sessions.Keys)
            {
                _sessions[index].Send(data);
                _sessions[index].Sent++;
            }
        }

        public void BroadCasttoOthers(byte[] data, int sessionId)
        {
            foreach (int index in _sessions.Keys)
            {
                if (index == sessionId)
                    continue;
                _sessions[index].Send(data);
                _sessions[index].Sent++;
            }
        }
    }
}