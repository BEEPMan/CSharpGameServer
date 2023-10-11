using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using ServerCore;

namespace GameServer
{
    public class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }
        public string Username { get; set; }

        public int PacketCount { get; set; }

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
            int dataSize = header.size - headerSize;
            byte[] dataBytes = new byte[dataSize];
            Buffer.BlockCopy(buffer, headerSize, dataBytes, 0, dataSize);

            switch ((PacketType)header.packetType)
            {
                case PacketType.PKT_C_LOGIN:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        C_LOGIN packet = Serializer.Deserialize<C_LOGIN>(dataStream);
                        Handle_C_LOGIN(packet);
                    }
                    break;
                case PacketType.PKT_C_CHAT:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        C_CHAT packet = Serializer.Deserialize<C_CHAT>(dataStream);
                        Handle_C_CHAT(packet);
                        Console.WriteLine($"Packet Size: {header.size}");
                        PacketCount++;
                    }
                    break;
                default:
                    break;
            }
        }

        public void Handle_C_LOGIN(C_LOGIN data)
        {
            Username = data.Username;

            S_LOGIN packet = new S_LOGIN { PlayerId = SessionId, Username = data.Username };
            byte[] sendBuffer = Utils.SerializePacket(PacketType.PKT_S_LOGIN, packet);
            SessionManager.Instance.BroadCast(sendBuffer);

            Console.WriteLine($"[Session {SessionId}] Login: {data.Username}");
        }

        public void Handle_C_CHAT(C_CHAT data)
        {
            Room.Broadcast(this, data.Chat);

            //S_CHAT packet = new S_CHAT { PlayerId = SessionId, Chat = data.Chat };
            //byte[] sendBuffer = Utils.SerializePacket(PacketType.PKT_S_CHAT, packet);
            //SessionManager.Instance.BroadCasttoOthers(sendBuffer, SessionId);

            Console.WriteLine($"[User {SessionId}] Chat: {data.Chat}()");
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Program.Room.Enter(this);

            Console.WriteLine($"[{endPoint}] OnConnected");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            // TODO: User가 나갔음을 다른 User들에게 Broadcast
            SessionManager.Instance.RemoveSession(this);
            if (Room != null)
            {
                Room.Leave(this);
                Room = null;
            }
            Console.WriteLine($"[Session {SessionId}] Disconnected");
            Console.WriteLine($"[Session {SessionId}] PacketCount: {PacketCount}");
        }

        public override void OnSend(int numOfBytes)
        {
            
        }
    }
}
