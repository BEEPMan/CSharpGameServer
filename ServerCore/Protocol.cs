using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public enum PacketType
    {
        LOGIN = 1,
        CHAT = 2,
    }

    [ProtoBuf.ProtoContract]
    public class PacketHeader
    {
        [ProtoBuf.ProtoMember(1)]
        public ushort size { get; set; }
        [ProtoBuf.ProtoMember(2)]
        public ushort packetType { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class LoginPacket
    {
        [ProtoBuf.ProtoMember(1)]
        public string username { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public string password { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class ChatPacket
    {
        [ProtoBuf.ProtoMember(1)]
        public string chat { get; set; }
    }
}
