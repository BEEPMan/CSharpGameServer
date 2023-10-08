using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace GameServer
{
    public class GameRoom
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();

        public void Broadcast(ClientSession session, string chat)
        {
            S_CHAT packet = new S_CHAT { PlayerId = session.SessionId, Chat = chat };
            byte[] sendBuffer = Utils.SerializePacket(PacketType.PKT_S_CHAT, packet);

            lock (_lock)
            {
                foreach (ClientSession s in _sessions)
                    s.Send(sendBuffer);
            }
        }

        public void Enter(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
                session.Room = null;
            }
        }
    }
}
