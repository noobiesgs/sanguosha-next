using MemoryPack;

namespace Noobie.SanGuoSha.Network
{
    [MemoryPackable]
    [MemoryPackUnion(10000, typeof(GameResponse))]
    [MemoryPackUnion(10001, typeof(AskForCardUsageResponse))]

    [MemoryPackUnion(20000, typeof(GameUpdate))]
    [MemoryPackUnion(20001, typeof(StatusSync))]

    [MemoryPackUnion(30000, typeof(LobbyPacket))]
    [MemoryPackUnion(30001, typeof(LoginPacket))]
    [MemoryPackUnion(30002, typeof(RegisterPacket))]
    [MemoryPackUnion(30003, typeof(ChatPacket))]
    [MemoryPackUnion(30004, typeof(LoginResultPacket))]
    [MemoryPackUnion(30005, typeof(ServerDisconnectedPacket))]
    [MemoryPackUnion(30006, typeof(RegisterResultPacket))]
    [MemoryPackUnion(30007, typeof(PingPacket))]
    public abstract partial record GameDataPacket;

    [MemoryPackable]
    [MemoryPackUnion(10001, typeof(AskForCardUsageResponse))]
    public abstract partial record GameResponse : GameDataPacket
    {
        public int Id { get; init; }
    }

    [MemoryPackable]
    public sealed partial record AskForCardUsageResponse : GameResponse;

    [MemoryPackable]
    [MemoryPackUnion(20001, typeof(StatusSync))]
    public abstract partial record GameUpdate : GameDataPacket;

    [MemoryPackable]
    public sealed partial record StatusSync : GameUpdate
    {
        public int Status { get; init; }
    }
}
