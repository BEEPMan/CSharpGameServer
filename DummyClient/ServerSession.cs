using ProtoBuf;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    public struct Player
    {
        public float posX;
        public float posY;
        public float posZ;
    }

    public class ServerSession : PacketSession
    {
        public Dictionary<int, Player> Players = new Dictionary<int, Player>();

        public int SessionId { get; set; }

        public override void OnRecvPacket(byte[] buffer)
        {
            // Extract Header
            int headerSize = sizeof(ushort) * 2;
            byte[] headerBytes = new byte[headerSize];
            Buffer.BlockCopy(buffer, 0, headerBytes, 0, headerSize);

            PacketHeader header;
            using (MemoryStream headerStream = new MemoryStream(headerBytes))
            {
                header = Serializer.Deserialize<PacketHeader>(headerStream);
            }

            // Extract Data
            int dataSize = header.Size - headerSize;
            byte[] dataBytes = new byte[dataSize];
            Buffer.BlockCopy(buffer, headerSize, dataBytes, 0, dataSize);

            // Deserialize based on packet type
            switch ((PacketType)header.PacketType)
            {
                case PacketType.PKT_S_ENTERGAME:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        S_ENTERGAME packet = Serializer.Deserialize<S_ENTERGAME>(dataStream);
                        Handle_S_ENTERGAME(packet);
                    }
                    break;
                case PacketType.PKT_S_LEAVEGAME:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        S_LEAVEGAME data = Serializer.Deserialize<S_LEAVEGAME>(dataStream);
                        Handle_S_LEAVEGAME(data);
                    }
                    break;
                case PacketType.PKT_S_PLAYERLIST:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        S_PLAYERLIST data = Serializer.Deserialize<S_PLAYERLIST>(dataStream);
                        Handle_S_PLAYERLIST(data);
                    }
                    break;
                case PacketType.PKT_S_CHAT:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        S_CHAT data = Serializer.Deserialize<S_CHAT>(dataStream);
                        Handle_S_CHAT(data);
                    }
                    break;
                case PacketType.PKT_S_MOVE:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        S_MOVE data = Serializer.Deserialize<S_MOVE>(dataStream);
                        Handle_S_MOVE(data);
                    }
                    break;
                default:
                    Console.WriteLine($"Unknown packet type: {header.PacketType}");
                    break;
            }
        }

        public void Handle_S_ENTERGAME(S_ENTERGAME data)
        {
            if (data.PlayerId == SessionId)
                Console.WriteLine($"Login success");
            else if(Players.ContainsKey(data.PlayerId))
            {
                Players[data.PlayerId] = new Player { posX = data.PosX, posY = data.PosY, posZ = data.PosZ };
            }
            else
            {
                Players.Add(data.PlayerId, new Player { posX = data.PosX, posY = data.PosY, posZ = data.PosZ });
                Console.WriteLine($"[User {data.PlayerId}] Enter game");
            }
        }

        public void Handle_S_LEAVEGAME(S_LEAVEGAME data)
        {
            Players.Remove(data.PlayerId);
            Console.WriteLine($"[User {data.PlayerId}] Leave game");
        }

        public void Handle_S_PLAYERLIST(S_PLAYERLIST data)
        {
            Random rand = new Random();
            float x = (float)rand.NextDouble() * 20 - 10;
            float z = (float)rand.NextDouble() * 20 - 10;
            SessionId = data.PlayerId;
            Players.Add(SessionId, new Player { posX = x, posY = 1.0f, posZ = z });
            foreach (PlayerInfo player in data.Players)
            {
                if (player.playerId == data.PlayerId) continue;
                Players.Add(player.playerId, new Player { posX = player.posX, posY = player.posY, posZ = player.posZ });
            }
        }

        public void Handle_S_CHAT(S_CHAT data)
        {
            if (!Players.ContainsKey(data.PlayerId)) return;
            Console.WriteLine($"[User {data.PlayerId}] Chat: {data.Chat}");
        }

        public void Handle_S_MOVE(S_MOVE data)
        {
            if (!Players.ContainsKey(data.PlayerId)) return;
            Players[data.PlayerId] = new Player { posX = data.PosX, posY = data.PosY, posZ = data.PosZ };
            Console.WriteLine($"[User {data.PlayerId}] Move: ({data.PosX}, {data.PosY}, {data.PosZ})");
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Connected");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"Disconnected");
        }

        public override void OnSend(int numOfBytes)
        {

        }
    }
}
