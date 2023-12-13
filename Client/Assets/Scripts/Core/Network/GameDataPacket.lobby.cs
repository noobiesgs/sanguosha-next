#nullable enable
using System;
using MemoryPack;

namespace Noobie.SanGuoSha.Network
{
    [MemoryPackUnion(0x2000, typeof(ILobbyPacket))]
    [MemoryPackUnion(0x2100, typeof(ILobbyRequestPacket))]
    [MemoryPackUnion(0x2101, typeof(LoginRequestPacket))]
    [MemoryPackUnion(0x2102, typeof(RegisterRequestPacket))]
    [MemoryPackUnion(0x2200, typeof(ILobbyResponsePacket))]
    [MemoryPackUnion(0x2201, typeof(LoginResponsePacket))]
    [MemoryPackUnion(0x2202, typeof(RegisterResponsePacket))]
    [MemoryPackUnion(0x2300, typeof(ILobbyMessagePacket))]
    [MemoryPackUnion(0x2301, typeof(ChatPacket))]
    [MemoryPackUnion(0x2302, typeof(ServerDisconnectedPacket))]
    [MemoryPackUnion(0x2303, typeof(PingPacket))]
    public partial interface IGameDataPacket
    {

    }

    [MemoryPackable]
    [MemoryPackUnion(0x2100, typeof(ILobbyRequestPacket))]
    [MemoryPackUnion(0x2101, typeof(LoginRequestPacket))]
    [MemoryPackUnion(0x2102, typeof(RegisterRequestPacket))]
    [MemoryPackUnion(0x2200, typeof(ILobbyResponsePacket))]
    [MemoryPackUnion(0x2201, typeof(LoginResponsePacket))]
    [MemoryPackUnion(0x2202, typeof(RegisterResponsePacket))]
    [MemoryPackUnion(0x2300, typeof(ILobbyMessagePacket))]
    [MemoryPackUnion(0x2301, typeof(ChatPacket))]
    [MemoryPackUnion(0x2302, typeof(ServerDisconnectedPacket))]
    [MemoryPackUnion(0x2303, typeof(PingPacket))]
    public partial interface ILobbyPacket : IGameDataPacket
    {
    }

    [MemoryPackable]
    [MemoryPackUnion(0x2101, typeof(LoginRequestPacket))]
    [MemoryPackUnion(0x2102, typeof(RegisterRequestPacket))]
    public partial interface ILobbyRequestPacket : ILobbyPacket
    {
        int RequestId { get; init; }
    }

    [MemoryPackable]
    [MemoryPackUnion(0x2201, typeof(LoginResponsePacket))]
    [MemoryPackUnion(0x2202, typeof(RegisterResponsePacket))]
    public partial interface ILobbyResponsePacket : ILobbyPacket
    {
        int ResponseId { get; init; }
    }

    [MemoryPackable]
    [MemoryPackUnion(0x2301, typeof(ChatPacket))]
    [MemoryPackUnion(0x2302, typeof(ServerDisconnectedPacket))]
    [MemoryPackUnion(0x2303, typeof(PingPacket))]
    public partial interface ILobbyMessagePacket : ILobbyPacket
    {

    }

    [MemoryPackable]
    public sealed partial record LoginRequestPacket(int RequestId, string AccountName, string Password, int ProtocolVersion) : ILobbyRequestPacket;

    [MemoryPackable]
    public sealed partial record RegisterRequestPacket(int RequestId, string AccountName, string Nickname, string Password) : ILobbyRequestPacket;

    [MemoryPackable]
    public sealed partial record RegisterResponsePacket(int ResponseId, RegistrationStatus Status) : ILobbyResponsePacket;

    [MemoryPackable]
    public sealed partial record LoginResponsePacket : ILobbyResponsePacket
    {
        [MemoryPackConstructor]
        public LoginResponsePacket(int responseId, LoginStatus status, AccountPacket account, LoginToken token)
        {
            Status = status;
            Account = account;
            Token = token;
            ResponseId = responseId;
        }

        public LoginResponsePacket(int responseId, LoginStatus status)
        {
            Status = status;
            ResponseId = responseId;
        }

        public LoginStatus Status { get; init; }

        public AccountPacket? Account { get; init; }

        public LoginToken Token { get; init; }

        public int ResponseId { get; init; }

        public void Deconstruct(out LoginStatus status, out AccountPacket? account, out LoginToken token)
        {
            status = Status;
            account = Account;
            token = Token;
        }
    }

    [MemoryPackable]
    public sealed partial record ChatPacket(string Message) : ILobbyMessagePacket;

    [MemoryPackable]
    public sealed partial record PingPacket : ILobbyMessagePacket;

    [MemoryPackable]
    public sealed partial record ServerDisconnectedPacket(DisconnectReason Reason) : ILobbyMessagePacket;

    [MemoryPackable]
    public sealed partial record AccountPacket(string Nickname, string Title, int Wins, int Losses, int Escapes, AvatarShowPacket AvatarShow)
    {
        public static readonly AccountPacket Empty = new(string.Empty, string.Empty, 0, 0, 0, new AvatarShowPacket(0, 0, 0));
    }

    [MemoryPackable]
    public sealed partial record AvatarShowPacket(int AvatarIndex, int BorderIndex, int BackgroundIndex);

    [MemoryPackable]
    public partial struct LoginToken
    {
        public static readonly LoginToken Empty = default;

        public Guid TokenString { get; init; }
    }

    public enum DisconnectReason
    {
        LoggedInOnAnotherDevice
    }

    public enum RegistrationStatus : byte
    {
        Success,
        Invalid,
        AccountAlreadyExists,
        NicknameAlreadyExists,
    }


    public enum LoginStatus : byte
    {
        Success,
        OutdatedVersion,
        InvalidUserNameAndPassword,
        UnknownFailure,
    }

    public enum RoomState : byte
    {
        Waiting,
        Gaming
    }
}
