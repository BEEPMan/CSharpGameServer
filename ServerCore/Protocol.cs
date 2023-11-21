using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public enum PacketType
    {
        PKT_S_ENTERGAME = 1,
        PKT_C_LEAVEGAME = 2,
        PKT_S_LEAVEGAME = 3,
        PKT_S_PLAYERLIST = 4,
        PKT_C_CHAT = 5,
        PKT_S_CHAT = 6,
        PKT_C_MOVE = 7,
        PKT_S_MOVE = 8,
        PKT_C_MOVE_V2 = 9,
        PKT_S_MOVE_V2 = 10,
    }

    [ProtoBuf.ProtoContract]
    public struct PlayerInfo
    {
        [ProtoBuf.ProtoMember(1)]
        public int playerId;
        [ProtoBuf.ProtoMember(2)]
        public float posX;
        [ProtoBuf.ProtoMember(3)]
        public float posY;
        [ProtoBuf.ProtoMember(4)]
        public float posZ;
    }

    [ProtoBuf.ProtoContract]
    public class PacketHeader
    {
        [ProtoBuf.ProtoMember(1)]
        public ushort Size { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public ushort PacketType { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class S_ENTERGAME
    {
        [ProtoBuf.ProtoMember(1)]
        public int PlayerId { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public float PosX { get; set; }

        [ProtoBuf.ProtoMember(3)]
        public float PosY { get; set; }

        [ProtoBuf.ProtoMember(4)]
        public float PosZ { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class C_LEAVEGAME
    {
    }

    [ProtoBuf.ProtoContract]
    public class S_LEAVEGAME
    {
        [ProtoBuf.ProtoMember(1)]
        public int PlayerId { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class S_PLAYERLIST
    {
        [ProtoBuf.ProtoMember(1)]
        public int PlayerId { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public List<PlayerInfo> Players { get; set; } = new List<PlayerInfo>();
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

    [ProtoBuf.ProtoContract]
    public class C_MOVE
    {
        [ProtoBuf.ProtoMember(1)]
        public float PosX { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public float PosY { get; set; }

        [ProtoBuf.ProtoMember(3)]
        public float PosZ { get; set; }

        [ProtoBuf.ProtoMember(4)]
        public long TimeStamp { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class S_MOVE
    {
        [ProtoBuf.ProtoMember(1)]
        public int PlayerId { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public float PosX { get; set; }

        [ProtoBuf.ProtoMember(3)]
        public float PosY { get; set; }

        [ProtoBuf.ProtoMember(4)]
        public float PosZ { get; set; }

        [ProtoBuf.ProtoMember(5)]
        public long TimeStamp { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class C_MOVE_V2
    {
        [ProtoBuf.ProtoMember(1)]
        public float PosX { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public float PosY { get; set; }

        [ProtoBuf.ProtoMember(3)]
        public float PosZ { get; set; }

        [ProtoBuf.ProtoMember(4)]
        public float VelX { get; set; }

        [ProtoBuf.ProtoMember(5)]
        public float VelY { get; set; }

        [ProtoBuf.ProtoMember(6)]
        public float VelZ { get; set; }

        [ProtoBuf.ProtoMember(7)]
        public long TimeStamp { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class S_MOVE_V2
    {
        [ProtoBuf.ProtoMember(1)]
        public int PlayerId { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public float PosX { get; set; }

        [ProtoBuf.ProtoMember(3)]
        public float PosY { get; set; }

        [ProtoBuf.ProtoMember(4)]
        public float PosZ { get; set; }

        [ProtoBuf.ProtoMember(5)]
        public float VelX { get; set; }

        [ProtoBuf.ProtoMember(6)]
        public float VelY { get; set; }

        [ProtoBuf.ProtoMember(7)]
        public float VelZ { get; set; }

        [ProtoBuf.ProtoMember(8)]
        public long TimeStamp { get; set; }
    }
}
