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

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
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
                case PacketType.PKT_S_MOVE_V2:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        S_MOVE_V2 data = Serializer.Deserialize<S_MOVE_V2>(dataStream);
                        Handle_S_MOVE_V2(data);
                    }
                    break;
                case PacketType.PKT_S_MOVE_V3:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        S_MOVE_V3 data = Serializer.Deserialize<S_MOVE_V3>(dataStream);
                        Handle_S_MOVE_V3(data);
                    }
                    break;
                case PacketType.PKT_S_POS:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        S_POS data = Serializer.Deserialize<S_POS>(dataStream);
                        Handle_S_POS(data);
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
            }
        }

        public void Handle_S_LEAVEGAME(S_LEAVEGAME data)
        {
            SessionManager.Instance.Players.TryRemove(data.PlayerId, out _);
        }

        public void Handle_S_PLAYERLIST(S_PLAYERLIST data)
        {
            SessionId = data.PlayerId;
            SessionManager.Instance.Players.TryAdd(SessionId, new Player(SessionId, 0.0f, 1.0f, 0.0f));
            Task.WaitAll();
        }

        public void Handle_S_CHAT(S_CHAT data)
        {
            if (!SessionManager.Instance.Players.ContainsKey(data.PlayerId)) return;
        }

        public void Handle_S_MOVE(S_MOVE data)
        {
            if (!SessionManager.Instance.Players.ContainsKey(data.PlayerId)) return;
        }

        public void Handle_S_MOVE_V2(S_MOVE_V2 data)
        {
            if (!SessionManager.Instance.Players.ContainsKey(data.PlayerId)) return;
        }

        public void Handle_S_MOVE_V3(S_MOVE_V3 data)
        {
            if (!SessionManager.Instance.Players.ContainsKey(data.PlayerId)) return;
        }

        public void Handle_S_POS(S_POS data)
        {
            if (!SessionManager.Instance.Players.ContainsKey(data.PlayerId)) return;
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
        }

        public override void OnSend(int numOfBytes)
        {

        }
    }
}
