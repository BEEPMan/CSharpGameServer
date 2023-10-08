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
        public string Username { get; set; }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            int pos = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            pos += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + pos);
            pos += 2;

            byte[] byteArray = buffer.ToArray();
            //int headerSize = sizeof(ushort) * 2;
            //byte[] headerBytes = new byte[headerSize];
            //Buffer.BlockCopy(buffer.Array, 0, headerBytes, 0, headerSize);

            //PacketHeader header;
            //using (MemoryStream headerStream = new MemoryStream(headerBytes))
            //{
            //    header = Serializer.Deserialize<PacketHeader>(headerStream);
            //}

            //// Extract Data
            //int dataSize = header.size;
            //byte[] dataBytes = new byte[dataSize];
            //Buffer.BlockCopy(buffer.Array, headerSize, dataBytes, 0, dataSize);

            switch ((PacketType)id)
            {
                case PacketType.PKT_C_LOGIN:
                    using (MemoryStream dataStream = new MemoryStream(byteArray))
                    {
                        C_LOGIN packet = Serializer.Deserialize<C_LOGIN>(dataStream);
                        Handle_C_LOGIN(packet);
                    }
                    break;
                case PacketType.PKT_C_CHAT:
                    using (MemoryStream dataStream = new MemoryStream(byteArray))
                    {
                        C_CHAT packet = Serializer.Deserialize<C_CHAT>(dataStream);
                        Handle_C_CHAT(packet);
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
            ArraySegment<byte> sendBuffer = SerializePacket(PacketType.PKT_S_LOGIN, packet);
            SessionManager.Instance.BroadCasttoOthers(sendBuffer, SessionId);

            Console.WriteLine($"[Session {SessionId}] Login: {data.Username}");
        }

        public void Handle_C_CHAT(C_CHAT data)
        {
            S_CHAT packet = new S_CHAT { PlayerId = SessionId, Chat = data.Chat };
            ArraySegment<byte> sendBuffer = SerializePacket(PacketType.PKT_S_CHAT, packet);
            SessionManager.Instance.BroadCasttoOthers(sendBuffer, SessionId);

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
