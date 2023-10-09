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

        public bool isLogin = false;

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
            int dataSize = header.size;
            byte[] dataBytes = new byte[dataSize];
            Buffer.BlockCopy(buffer, headerSize, dataBytes, 0, dataSize);

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
            if(data.Username == Username)
                Console.WriteLine($"Login success");
            else
                Console.WriteLine($"[User {data.Username}] Login");
        }

        public void Handle_S_CHAT(S_CHAT data)
        {
            Console.WriteLine($"[User {data.PlayerId}] Chat: {data.Chat}");
        }

        public override void OnConnected(EndPoint endPoint)
        {
            //Console.Write($"Input username: ");
            //string data = Console.ReadLine();

            //Username = data;
            //C_LOGIN packet = new C_LOGIN { Username = data };
            //byte[] sendBuffer = Utils.SerializePacket(PacketType.PKT_C_LOGIN, packet);
            //Send(sendBuffer);

            isLogin = true;
            Console.WriteLine($"Connected");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"Disconnected");
        }

        public override void OnSend(int numOfBytes)
        {

        }
    }
}
