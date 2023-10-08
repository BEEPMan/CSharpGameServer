using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Utils
    {
        public static byte[] SerializePacket(PacketType packetType, object data)
        {
            byte[] headerBytes, dataBytes;

            using (MemoryStream dataStream = new MemoryStream())
            {
                Serializer.Serialize(dataStream, data);
                dataBytes = dataStream.ToArray();
            }

            PacketHeader header = new PacketHeader { packetType = (ushort)packetType, size = (ushort)dataBytes.Length };
            using (MemoryStream headerStream = new MemoryStream())
            {
                Serializer.Serialize(headerStream, header);
                headerBytes = headerStream.ToArray();
            }

            byte[] finalPacket = new byte[headerBytes.Length + dataBytes.Length];
            Buffer.BlockCopy(headerBytes, 0, finalPacket, 0, headerBytes.Length);
            Buffer.BlockCopy(dataBytes, 0, finalPacket, headerBytes.Length, dataBytes.Length);

            return finalPacket;
        }
    }
}
