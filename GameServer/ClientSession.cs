using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using ServerCore;
using static System.Collections.Specialized.BitVector32;

namespace GameServer
{
    public class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }

        object _lock = new object();

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            byte[] recvPacket = new byte[buffer.Count];
            Buffer.BlockCopy(buffer.Array, buffer.Offset, recvPacket, 0, buffer.Count);

            if (Room == null)
            {
                return;
            }

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

            switch ((PacketType)header.PacketType)
            {
                case PacketType.PKT_C_LEAVEGAME:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        C_LEAVEGAME data = Serializer.Deserialize<C_LEAVEGAME>(dataStream);
                        Handle_C_LEAVEGAME(data);
                    }
                    break;
                case PacketType.PKT_C_CHAT:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        C_CHAT data = Serializer.Deserialize<C_CHAT>(dataStream);
                        Handle_C_CHAT(data);
                    }
                    break;
                case PacketType.PKT_C_MOVE:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        C_MOVE data = Serializer.Deserialize<C_MOVE>(dataStream);
                        Handle_C_MOVE(data);
                    }
                    break;
                default:
                    Console.WriteLine($"Unknown packet type: {header.PacketType}");
                    break;
            }
        }

        public void Handle_C_LEAVEGAME(C_LEAVEGAME data)
        {
            S_LEAVEGAME packet = new S_LEAVEGAME { PlayerId = SessionId };
            byte[] sendBuffer = Utils.SerializePacket(PacketType.PKT_S_LEAVEGAME, packet);
            GameRoom room = Room;
            room.Push(() => { room.Broadcast(sendBuffer); });

            Console.WriteLine($"[User {SessionId}] Left Game");
            SessionManager.Instance.RemoveSession(this);
            if (Room != null)
            {
                room = Room;
                room.Push(() => { room.Leave(this); });   
                Room = null;
            }
        }

        public void Handle_C_CHAT(C_CHAT data)
        {
            S_CHAT packet = new S_CHAT { PlayerId = SessionId, Chat = data.Chat };
            byte[] sendBuffer = Utils.SerializePacket(PacketType.PKT_S_CHAT, packet);
            GameRoom room = Room;
            room.Push(() => { room.Broadcast(sendBuffer); });

            Console.WriteLine($"[User {SessionId}] Chat: {data.Chat}");
        }

        public void Handle_C_MOVE(C_MOVE data)
        {
            GameRoom room = Room;
            room.Push(() => { room.Move(SessionId, data.PosX, data.PosY, data.PosZ); });

            S_MOVE packet = new S_MOVE
            {
                PlayerId = SessionId,
                PosX = data.PosX,
                PosY = data.PosY,
                PosZ = data.PosZ,
                VelX = data.VelX,
                VelY = data.VelY,
                VelZ = data.VelZ,
                TimeStamp = data.TimeStamp
            };
            byte[] sendBuffer = Utils.SerializePacket(PacketType.PKT_S_MOVE, packet);
            room.Push(() => { room.Broadcast(sendBuffer); });
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Random rand = new Random();
            float x = (float)rand.NextDouble() * 20 - 10;
            float z = (float)rand.NextDouble() * 20 - 10;

            GameRoom room = Program.Room;
            room.Push(() => { room.Enter(this, x, 1.0f, z); });

            S_ENTERGAME enter = new S_ENTERGAME { PlayerId = SessionId, PosX = x, PosY = 1.0f, PosZ = z };
            byte[] enterGamePacket = Utils.SerializePacket(PacketType.PKT_S_ENTERGAME, enter);
            room.Push(() => { room.Broadcast(enterGamePacket); });

            Console.WriteLine($"[{endPoint}] OnConnected");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"Session #{SessionId}: Disconnected");
        }

        public override void OnSend(int numOfBytes)
        {
            
        }
    }
}
