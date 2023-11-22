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
        public static ArraySegment<byte> SerializePacket(PacketType packetType, object data)
        {
            byte[] headerBytes = new byte[sizeof(ushort) * 2];
            byte[] dataBytes;

            using (MemoryStream dataStream = new MemoryStream())
            {
                Serializer.Serialize(dataStream, data);
                dataBytes = dataStream.ToArray();
            }

            ushort size = (ushort)(dataBytes.Length + sizeof(ushort) * 2);
            BitConverter.TryWriteBytes(new Span<byte>(headerBytes, 0, sizeof(ushort)), size);
            BitConverter.TryWriteBytes(new Span<byte>(headerBytes, sizeof(ushort), sizeof(ushort)), (ushort)packetType);

            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            Array.Copy(headerBytes, 0, openSegment.Array, openSegment.Offset, headerBytes.Length);
            Array.Copy(dataBytes, 0, openSegment.Array, openSegment.Offset + headerBytes.Length, dataBytes.Length);
            ArraySegment<byte> segment = SendBufferHelper.Close(headerBytes.Length + dataBytes.Length);

            return segment;
        }
    }
}
