using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public enum PacketType
    {
        PKT_C_LOGIN = 1,
        PKT_S_LOGIN = 2,
        PKT_C_CHAT = 3,
        PKT_S_CHAT = 4,
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
    public class C_LOGIN
    {
        [ProtoBuf.ProtoMember(1)]
        public string Username { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class S_LOGIN
    {
        [ProtoBuf.ProtoMember(1)]
        public int PlayerId { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public string Username { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class C_CHAT
    {
        [ProtoBuf.ProtoMember(1)]
        public string Chat { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class S_CHAT
    {
        [ProtoBuf.ProtoMember(1)]
        public int PlayerId { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public string Chat { get; set; }
    }
}
