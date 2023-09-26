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
    class ClientSession : PacketSession
    {
        public ClientSession(Socket clientSocket) : base(clientSocket)
        {
        }

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
                case PacketType.LOGIN:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        LoginPacket loginPacket = Serializer.Deserialize<LoginPacket>(dataStream);
                        // Handle Login
                    }
                    break;
                case PacketType.CHAT:
                    using (MemoryStream dataStream = new MemoryStream(dataBytes))
                    {
                        ChatPacket chatPacket = Serializer.Deserialize<ChatPacket>(dataStream);
                        
                    }
                    break;
                default:
                    break;
            }
        }

        public byte[] SerializePacket(PacketType packetType, object packet)
        {
            byte[] headerBytes, dataBytes;

            // Serialize Data
            using (MemoryStream dataStream = new MemoryStream())
            {
                Serializer.Serialize(dataStream, packet);
                dataBytes = dataStream.ToArray();
            }

            // Serialize Header
            PacketHeader header = new PacketHeader { packetType = (ushort)packetType, size = (ushort)dataBytes.Length };
            using (MemoryStream headerStream = new MemoryStream())
            {
                Serializer.Serialize(headerStream, header);
                headerBytes = headerStream.ToArray();
            }

            // Combine Header and Data
            byte[] finalPacket = new byte[headerBytes.Length + dataBytes.Length];
            Buffer.BlockCopy(headerBytes, 0, finalPacket, 0, headerBytes.Length);
            Buffer.BlockCopy(dataBytes, 0, finalPacket, headerBytes.Length, dataBytes.Length);

            return finalPacket;
        }

        public void HandleChat(ChatPacket data)
        {
            ChatPacket packet = new ChatPacket { chat = data.chat };
            byte[] sendBuffer = SerializePacket(PacketType.CHAT, packet);
            SessionManager.Instance.BroadCast(sendBuffer);
        }

        public override void OnConnected(EndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public override void OnSend(int numOfBytes)
        {
            throw new NotImplementedException();
        }
    }
}
