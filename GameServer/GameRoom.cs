using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace GameServer
{
    public struct Player
    {
        public float posX;
        public float posY;
        public float posZ;
    }

    public class GameRoom
    {
        public Dictionary<int, Player> Players = new Dictionary<int, Player>();
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();

        public void Broadcast(byte[] packet)
        {
            lock (_lock)
            {
                foreach (ClientSession s in _sessions)
                {
                    s.Send(packet);
                    // s.Sent++;
                }
            }
        }

        public void BroadcastEnterGame(byte[] packet)
        {
            lock (_lock)
            {
                foreach (ClientSession s in _sessions)
                {
                    s.Send(packet);
                    s.Sent++;
                    Thread.Sleep(100);
                }
            }
        }

        public void Enter(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Add(session);
                Players.Add(session.SessionId, new Player { posX = 0, posY = 1, posZ = 0 });
                session.Room = this;

                S_PLAYERLIST players = new S_PLAYERLIST();
                players.PlayerId = session.SessionId;
                foreach (KeyValuePair<int, Player> kv in Players)
                {
                    Player player = kv.Value;
                    PlayerInfo playerInfo = new PlayerInfo { playerId = kv.Key, posX = player.posX, posY = player.posY, posZ = player.posZ };
                    players.Players.Add(playerInfo);
                }
                byte[] playerListPacket = Utils.SerializePacket(PacketType.PKT_S_PLAYERLIST, players);
                session.Send(playerListPacket);
                // session.Sent++;

                
            }
        }

        public void Leave(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
                Players.Remove(session.SessionId);
                session.Room = null;
            }
        }

        public void Move(int sessionId, float posX, float posY, float posZ)
        {
            lock (_lock)
            {
                Player player = Players[sessionId];
                player.posX = posX;
                player.posY = posY;
                player.posZ = posZ;
                Players[sessionId] = player;
            }
        }
    }
}
