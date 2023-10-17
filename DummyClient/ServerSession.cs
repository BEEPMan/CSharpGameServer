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

        // public int Received { get; set; }
        // public int Sent { get; set; }

        StreamWriter _log;

        object _lock = new object();

        public override void OnRecvPacket(byte[] buffer)
        {
            // Received++;

            // Extract Header
            int headerSize = sizeof(ushort) * 2;
            byte[] headerBytes = new byte[headerSize];
            Buffer.BlockCopy(buffer, 0, headerBytes, 0, headerSize);

            PacketHeader header = new PacketHeader();
            header.Size = BitConverter.ToUInt16(headerBytes, 0);
            header.PacketType = BitConverter.ToUInt16(headerBytes, sizeof(ushort));

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
                        // lock (_lock) { Received++; }
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
                    Console.WriteLine($"Session #{SessionId}: Unknown packet type: {header.PacketType}");
                    break;
            }
        }

        public void Handle_S_ENTERGAME(S_ENTERGAME data)
        {
            if (data.PlayerId == SessionId)
            {
                _log.WriteLine($"Login success");
                // Console.WriteLine($"Session #{SessionId}: Login success");
            }
            else
            {
                Players.Add(data.PlayerId, new Player { posX = data.PosX, posY = data.PosY, posZ = data.PosZ });
                _log.WriteLine($"[User {data.PlayerId}] Enter game");
                // Console.WriteLine($"Session #{SessionId}: [User {data.PlayerId}] Enter game");
            }
        }

        public void Handle_S_LEAVEGAME(S_LEAVEGAME data)
        {
            Players.Remove(data.PlayerId);
            _log.WriteLine($"[User {data.PlayerId}] Leave game");
            // Console.WriteLine($"Session #{SessionId}: [User {data.PlayerId}] Leave game");
        }

        public void Handle_S_PLAYERLIST(S_PLAYERLIST data)
        {
            Random rand = new Random();
            float x = (float)rand.NextDouble() * 20 - 10;
            float z = (float)rand.NextDouble() * 20 - 10;
            SessionId = data.PlayerId;
            Players.Add(SessionId, new Player { posX = x, posY = 1.0f, posZ = z });
            _log = new StreamWriter($"C:\\Logs/log_{SessionId}.txt");
            _log.WriteLine($"[PlayerList]");
            foreach (PlayerInfo player in data.Players)
            {
                if (player.playerId == data.PlayerId) continue;
                Players.Add(player.playerId, new Player { posX = player.posX, posY = player.posY, posZ = player.posZ });
                _log.WriteLine($"User {player.playerId}: ({player.posX}, {player.posY}, {player.posZ})");
            }
            _log.WriteLine($"[EndList]");
        }

        public void Handle_S_CHAT(S_CHAT data)
        {
            if (!Players.ContainsKey(data.PlayerId)) return;
            _log.WriteLine($"[User {data.PlayerId}] Chat: {data.Chat}");
            // Console.WriteLine($"Session #{SessionId}: [User {data.PlayerId}] Chat: {data.Chat}");
        }

        public void Handle_S_MOVE(S_MOVE data)
        {
            if (!Players.ContainsKey(data.PlayerId)) return;
            Players[data.PlayerId] = new Player { posX = data.PosX, posY = data.PosY, posZ = data.PosZ };
            _log.WriteLine($"[User {data.PlayerId}] Move: ({data.PosX}, {data.PosY}, {data.PosZ})");
            // Console.WriteLine($"Session #{SessionId}: [User {data.PlayerId}] Move: ({data.PosX}, {data.PosY}, {data.PosZ})");
        }

        public override void OnConnected(EndPoint endPoint)
        {
            // Console.WriteLine($"Session #{SessionId}: Connected");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            _log.WriteLine($"Session #{SessionId}: Disconnected(Total Send Packet: {Sent}, Total Receive Packet: {Received})");
            // Console.WriteLine($"Session #{SessionId}: Disconnected(Total Send Packet: {Sent}, Total Receive Packet: {Received})");
            _log.Close();
        }

        public override void OnSend(int numOfBytes)
        {

        }
    }
}
