using ProtoBuf;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    public class ServerSession : PacketSession
    {
        

        public int SessionId { get; set; }

        // public int Received { get; set; }
        // public int Sent { get; set; }

        // Logger _log;
        StreamWriter _log;

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            // Received++;
            byte[] recvPacket = new byte[buffer.Count];
            Buffer.BlockCopy(buffer.Array, buffer.Offset, recvPacket, 0, buffer.Count);

            // Extract Header
            int headerSize = sizeof(ushort) * 2;
            byte[] headerBytes = new byte[headerSize];
            Buffer.BlockCopy(recvPacket, 0, headerBytes, 0, headerSize);

            PacketHeader header = new PacketHeader();
            header.Size = BitConverter.ToUInt16(headerBytes, 0);
            header.PacketType = BitConverter.ToUInt16(headerBytes, sizeof(ushort));

            // Extract Data
            int dataSize = header.Size - headerSize;
            byte[] dataBytes = new byte[dataSize];
            Buffer.BlockCopy(recvPacket, headerSize, dataBytes, 0, dataSize);

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
                SessionManager.Instance.Players[SessionId].SetPosition(data.PosX, data.PosY, data.PosZ);
                _log.WriteLine($"Login success");
                // Task.Run(() => _log.WriteAsync($"Login success"));
            }
            else
            {
                _log.WriteLine($"[User {data.PlayerId}] Enter game");
                // Task.Run(() => _log.WriteAsync($"[User {data.PlayerId}] Enter game"));
            }
        }

        public void Handle_S_LEAVEGAME(S_LEAVEGAME data)
        {
            SessionManager.Instance.Players.TryRemove(data.PlayerId, out _);
            _log.WriteLine($"[User {data.PlayerId}] Leave game");
            // Task.Run(() => _log.WriteAsync($"[User {data.PlayerId}] Leave game"));
        }

        public void Handle_S_PLAYERLIST(S_PLAYERLIST data)
        {
            SessionId = data.PlayerId;
            SessionManager.Instance.Players.TryAdd(SessionId, new Player(0.0f, 1.0f, 0.0f));
            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + $"\\..\\..\\Logs/";
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
            _log = new StreamWriter(path + $"log_{SessionId}.txt");
            _log.WriteLine($"[PlayerList]");
            // _log = new Logger($"C:\\Logs/log_{SessionId}.txt");
            // Task.Run(() => _log.WriteAsync($"[PlayerList]"));
            foreach (PlayerInfo player in data.Players)
            {
                _log.WriteLine($"User {player.playerId}: ({player.posX}, {player.posY}, {player.posZ})");
            }
            _log.WriteLine($"[EndList]");
            // Task.Run(() => _log.WriteAsync($"[EndList]"));
            Task.WaitAll();
        }

        public void Handle_S_CHAT(S_CHAT data)
        {
            if (!SessionManager.Instance.Players.ContainsKey(data.PlayerId)) return;
            _log.WriteLine($"[User {data.PlayerId}] Chat: {data.Chat}");
            // Task.Run(() => _log.WriteAsync($"[User {data.PlayerId}] Chat: {data.Chat}"));
        }

        public void Handle_S_MOVE(S_MOVE data)
        {
            if (!SessionManager.Instance.Players.ContainsKey(data.PlayerId)) return;
            _log.WriteLine($"[User {data.PlayerId}] Move: ({data.PosX}, {data.PosY}, {data.PosZ}) ({data.VelX}, {data.VelY}, {data.VelZ})");
            // Task.Run(() => _log.WriteAsync($"[User {data.PlayerId}] Move: ({data.PosX}, {data.PosY}, {data.PosZ})"));
        }

        public override void OnConnected(EndPoint endPoint)
        {

        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            if (SessionId == 1)
            {
                Console.WriteLine($"Received");
            }
            _log.WriteLine($"Session #{SessionId}: Disconnected(Total Send Packet: {Sent}, Total Receive Packet: {Received})");
            // Task.Run(() => _log.WriteAsync($"Session #{SessionId}: Disconnected(Total Send Packet: {Sent}, Total Receive Packet: {Received})"));
            _log.Flush();
            _log.Close();
            // Task.Run(() => _log.CloseAsync());
            // Task.WaitAll();
        }

        public override void OnSend(int numOfBytes)
        {

        }
    }
}
