using ProtoBuf;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    public class ServerSession : PacketSession
    {
        public int SessionId { get; set; }
        public string Username { get; set; }

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
                case PacketType.PKT_S_LOGIN:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        S_LOGIN packet = Serializer.Deserialize<S_LOGIN>(dataStream);
                        Handle_S_LOGIN(packet);
                    }
                    break;
                case PacketType.PKT_S_CHAT:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        S_CHAT packet = Serializer.Deserialize<S_CHAT>(dataStream);
                        Handle_S_CHAT(packet);
                    }
                    break;
                default:
                    break;
            }
        }

        public void Handle_S_LOGIN(S_LOGIN data)
        {
            Username = data.Username;

            C_LOGIN packet = new C_LOGIN { Username = data.Username };
            byte[] sendBuffer = SerializePacket(PacketType.PKT_C_LOGIN, packet);
            // TODO : Send to Server

            Console.WriteLine($"[Session {SessionId}] Login: {data.Username}");
        }

        public void Handle_S_CHAT(S_CHAT data)
        {
            C_CHAT packet = new C_CHAT { Chat = data.Chat };
            byte[] sendBuffer = SerializePacket(PacketType.PKT_C_CHAT, packet);
            // TODO : Send to Server

            Console.WriteLine($"[User {Username}] Chat: {data.Chat}");
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
