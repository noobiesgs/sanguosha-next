using MemoryPack;

namespace Noobie.SanGuoSha.Network
{
    [MemoryPackable]
    [MemoryPackUnion(0x1000, typeof(IGamingPacket))]
    [MemoryPackUnion(0x1100, typeof(IGameResponse))]
    [MemoryPackUnion(0x1101, typeof(AskForCardUsageResponse))]
    [MemoryPackUnion(0x1200, typeof(IGameUpdate))]
    [MemoryPackUnion(0x1201, typeof(StatusSync))]
    public partial interface IGameDataPacket
    {

    }

    [MemoryPackable]
    [MemoryPackUnion(0x1100, typeof(IGameResponse))]
    [MemoryPackUnion(0x1101, typeof(AskForCardUsageResponse))]

    [MemoryPackUnion(0x1200, typeof(IGameUpdate))]
    [MemoryPackUnion(0x1201, typeof(StatusSync))]
    public partial interface IGamingPacket : IGameDataPacket
    {
        float Timestamp { get; init; }
    }

    [MemoryPackable]
    [MemoryPackUnion(0x1101, typeof(AskForCardUsageResponse))]
    public partial interface IGameResponse : IGamingPacket
    {
        int Id { get; init; }
    }

    [MemoryPackable]
    public sealed partial record AskForCardUsageResponse(int Id, float Timestamp) : IGameResponse;

    [MemoryPackable]
    [MemoryPackUnion(0x1201, typeof(StatusSync))]
    public partial interface IGameUpdate : IGamingPacket
    {

    }

    [MemoryPackable]
    public sealed partial record StatusSync(float Timestamp, int Status) : IGameUpdate;
}
