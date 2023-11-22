using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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

    public class GameRoom : IJobQueue
    {
        public Dictionary<int, Player> Players = new Dictionary<int, Player>();
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();
        JobQueue _jobQueue = new JobQueue();
        List<byte[]> _pendingList = new List<byte[]>();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Broadcast(byte[] packet)
        {
            _pendingList.Add(packet);

            //foreach (ClientSession s in _sessions)
            //{
            //    s.Send(packet);
            //}
        }

        public void Flush()
        {
            foreach (ClientSession s in _sessions)
            {
                s.Send(_pendingList);
            }
            _pendingList.Clear();
        }

        public void Enter(ClientSession session, float x, float y, float z)
        {
            _sessions.Add(session);
            Players.Add(session.SessionId, new Player { posX = x, posY = y, posZ = z });
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
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
            Players.Remove(session.SessionId);
            session.Room = null;
        }

        public void Move(int sessionId, float posX, float posY, float posZ)
        {
            Player player = Players[sessionId];
            player.posX = posX;
            player.posY = posY;
            player.posZ = posZ;
            Players[sessionId] = player;
        }
    }
}
