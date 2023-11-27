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
                case PacketType.PKT_C_MOVE_V2:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        C_MOVE_V2 data = Serializer.Deserialize<C_MOVE_V2>(dataStream);
                        Handle_C_MOVE_V2(data);
                    }
                    break;
                case PacketType.PKT_C_MOVE_V3:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        C_MOVE_V3 data = Serializer.Deserialize<C_MOVE_V3>(dataStream);
                        Handle_C_MOVE_V3(data);
                    }
                    break;
                case PacketType.PKT_C_POS:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        C_POS data = Serializer.Deserialize<C_POS>(dataStream);
                        Handle_C_POS(data);
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
            ArraySegment<byte> sendBuffer = Utils.SerializePacket(PacketType.PKT_S_LEAVEGAME, packet);
            Program.Room.Push(() => { Program.Room.Broadcast(sendBuffer); });

            Console.WriteLine($"[User {SessionId}] Left Game");
            SessionManager.Instance.RemoveSession(this);
            if (Room != null)
            {
                Program.Room.Push(() => { Program.Room.Leave(this); });
                Room = null;
            }
        }

        public void Handle_C_CHAT(C_CHAT data)
        {
            S_CHAT packet = new S_CHAT { PlayerId = SessionId, Chat = data.Chat };
            ArraySegment<byte> sendBuffer = Utils.SerializePacket(PacketType.PKT_S_CHAT, packet);
            Room.Push(() => { Room.Broadcast(sendBuffer); });

            Console.WriteLine($"[User {SessionId}] Chat: {data.Chat}");
        }

        public void Handle_C_MOVE(C_MOVE data)
        {
            Room.Push(() => { Room.Move(SessionId, data.PosX, data.PosY, data.PosZ); });

            S_MOVE packet = new S_MOVE
            {
                PlayerId = SessionId,
                PosX = data.PosX,
                PosY = data.PosY,
                PosZ = data.PosZ,
                TimeStamp = data.TimeStamp
            };
            ArraySegment<byte> sendBuffer = Utils.SerializePacket(PacketType.PKT_S_MOVE, packet);
            Room.Push(() => { Room.BroadcastMove(sendBuffer, SessionId); });
        }

        public void Handle_C_MOVE_V2(C_MOVE_V2 data)
        {
            Room.Push(() => { Room.Move(SessionId, data.PosX, data.PosY, data.PosZ); });

            S_MOVE_V2 packet = new S_MOVE_V2
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
            ArraySegment<byte> sendBuffer = Utils.SerializePacket(PacketType.PKT_S_MOVE_V2, packet);
            Room.Push(() => { Room.BroadcastMove(sendBuffer, SessionId); });
        }

        public void Handle_C_MOVE_V3(C_MOVE_V3 data)
        {
            S_MOVE_V3 packet = new S_MOVE_V3
            {
                PlayerId = SessionId,
                Theta = data.Theta,
                Speed = data.Speed
            };
            ArraySegment<byte> sendBuffer = Utils.SerializePacket(PacketType.PKT_S_MOVE_V3, packet);
            Room.Push(() => { Room.BroadcastMove(sendBuffer, SessionId); });
        }

        public void Handle_C_POS(C_POS data)
        {
            Room.Push(() => { Room.Move(SessionId, data.PosX, data.PosY, data.PosZ); });

            S_POS packet = new S_POS
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
            ArraySegment<byte> sendBuffer = Utils.SerializePacket(PacketType.PKT_S_POS, packet);
            Room.Push(() => { Room.BroadcastMove(sendBuffer, SessionId); });
        }

        public override void OnConnected(EndPoint endPoint)
        {
            //Random rand = new Random();
            //float x = (float)rand.NextDouble() * 20 - 10;
            //float z = (float)rand.NextDouble() * 20 - 10;
            float x = 0.0f;
            float z = 0.0f;

            Program.Room.Push(() => { Program.Room.Enter(this, x, 1.0f, z); });

            S_ENTERGAME enter = new S_ENTERGAME { PlayerId = SessionId, PosX = x, PosY = 1.0f, PosZ = z };
            ArraySegment<byte> enterGamePacket = Utils.SerializePacket(PacketType.PKT_S_ENTERGAME, enter);
            Program.Room.Push(() => { Program.Room.Broadcast(enterGamePacket); });

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
