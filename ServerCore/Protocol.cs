using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public enum PacketType
    {
        PKT_S_LOGIN = 1,
        PKT_C_LOGIN = 2,
        PKT_S_CHAT = 3,
        PKT_C_CHAT = 4,
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
    }

    [ProtoBuf.ProtoContract]
    public class ChatPacket
    {
        [ProtoBuf.ProtoMember(1)]
        public string chat { get; set; }
    }
}
