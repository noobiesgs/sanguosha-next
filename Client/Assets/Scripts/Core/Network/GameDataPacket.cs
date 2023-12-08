using MemoryPack;

namespace Noobie.SanGuoSha.Network
{
    [MemoryPackable]
    [MemoryPackUnion(10000, typeof(IGamingPacket))]
    [MemoryPackUnion(11000, typeof(IGameResponse))]
    [MemoryPackUnion(11001, typeof(AskForCardUsageResponse))]
    [MemoryPackUnion(12000, typeof(IGameUpdate))]
    [MemoryPackUnion(12001, typeof(StatusSync))]

    [MemoryPackUnion(20000, typeof(ILobbyPacket))]
    [MemoryPackUnion(20001, typeof(LoginPacket))]
    [MemoryPackUnion(20002, typeof(RegisterPacket))]
    [MemoryPackUnion(20003, typeof(ChatPacket))]
    [MemoryPackUnion(20004, typeof(LoginResultPacket))]
    [MemoryPackUnion(20005, typeof(ServerDisconnectedPacket))]
    [MemoryPackUnion(20006, typeof(RegisterResultPacket))]
    [MemoryPackUnion(20007, typeof(PingPacket))]
    public partial interface IGameDataPacket
    {

    }

    [MemoryPackable]
    [MemoryPackUnion(11000, typeof(IGameResponse))]
    [MemoryPackUnion(11001, typeof(AskForCardUsageResponse))]

    [MemoryPackUnion(12000, typeof(IGameUpdate))]
    [MemoryPackUnion(12001, typeof(StatusSync))]
    public partial interface IGamingPacket : IGameDataPacket
    {

    }

    [MemoryPackable]
    [MemoryPackUnion(11001, typeof(AskForCardUsageResponse))]
    public partial interface IGameResponse : IGamingPacket
    {
        int Id { get; init; }
    }

    [MemoryPackable]
    public sealed partial record AskForCardUsageResponse(int Id) : IGameResponse;

    [MemoryPackable]
    [MemoryPackUnion(12001, typeof(StatusSync))]
    public partial interface IGameUpdate : IGamingPacket
    {

    }

    [MemoryPackable]
    public sealed partial record StatusSync : IGameUpdate
    {
        public int Status { get; init; }
    }
}
