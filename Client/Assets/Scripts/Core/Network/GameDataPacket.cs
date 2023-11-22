using MemoryPack;

namespace Noobie.SanGuoSha.Network
{
    [MemoryPackable]
    [MemoryPackUnion(10000, typeof(GameResponse))]
    [MemoryPackUnion(10001, typeof(AskForCardUsageResponse))]

    [MemoryPackUnion(20000, typeof(GameUpdate))]
    [MemoryPackUnion(20001, typeof(StatusSync))]
    public abstract partial record GameDataPacket;

    [MemoryPackable]
    [MemoryPackUnion(20001, typeof(StatusSync))]
    public abstract partial record GameUpdate : GameDataPacket;

    [MemoryPackable]
    [MemoryPackUnion(10001, typeof(AskForCardUsageResponse))]
    public abstract partial record GameResponse : GameDataPacket
    {
        public int Id { get; init; }
    }

    [MemoryPackable]
    public sealed partial record AskForCardUsageResponse : GameResponse;

    [MemoryPackable]
    public sealed partial record StatusSync : GameUpdate
    {
        public int Status { get; init; }
    }
}
