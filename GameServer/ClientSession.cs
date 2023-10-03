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
        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            // Extract Header
            int headerSize = sizeof(ushort) * 2;
            byte[] headerBytes = new byte[headerSize];
            Buffer.BlockCopy(buffer.Array, 0, headerBytes, 0, headerSize);

            PacketHeader header;
            using (MemoryStream headerStream = new MemoryStream(headerBytes))
            {
                header = Serializer.Deserialize<PacketHeader>(headerStream);
            }

            // Extract Data
            int dataSize = header.size;
            byte[] dataBytes = new byte[dataSize];
            Buffer.BlockCopy(buffer.Array, headerSize, dataBytes, 0, dataSize);

            // Deserialize based on packet type
            switch ((PacketType)header.packetType)
            {
                case PacketType.PKT_C_LOGIN:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        LoginPacket loginPacket = Serializer.Deserialize<LoginPacket>(dataStream);
                        // Handle Login
                    }
                    break;
                case PacketType.PKT_C_CHAT:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        ChatPacket chatPacket = Serializer.Deserialize<ChatPacket>(dataStream);
                        
                    }
                    break;
                default:
                    break;
            }
        }

        public void HandleLogin(LoginPacket data)
        {
            LoginPacket packet = new LoginPacket { username = data.username };
            byte[] sendBuffer = SerializePacket(PacketType.PKT_S_LOGIN, packet);
            SessionManager.Instance.BroadCast(sendBuffer);

            Console.WriteLine($"[Session {SessionManager.Instance.GetSessionIndex(this)}] Login: {data.username}");
        }

        public void HandleChat(ChatPacket data)
        {
            ChatPacket packet = new ChatPacket { chat = data.chat };
            byte[] sendBuffer = SerializePacket(PacketType.PKT_S_CHAT, packet);
            SessionManager.Instance.BroadCast(sendBuffer);
        }

        public override void OnConnected(EndPoint endPoint)
        {
            
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            // TODO: User가 나갔음을 다른 User들에게 Broadcast
        }

        public override void OnSend(int numOfBytes)
        {
            
        }
    }
}
