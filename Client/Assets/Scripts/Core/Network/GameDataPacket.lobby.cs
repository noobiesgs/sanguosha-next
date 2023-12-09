#nullable enable
using System;
using MemoryPack;

namespace Noobie.SanGuoSha.Network
{
    [MemoryPackable]
    [MemoryPackUnion(20001, typeof(LoginPacket))]
    [MemoryPackUnion(20002, typeof(RegisterPacket))]
    [MemoryPackUnion(20003, typeof(ChatPacket))]
    [MemoryPackUnion(20004, typeof(LoginResultPacket))]
    [MemoryPackUnion(20005, typeof(ServerDisconnectedPacket))]
    [MemoryPackUnion(20006, typeof(RegisterResultPacket))]
    [MemoryPackUnion(20007, typeof(PingPacket))]
    public partial interface ILobbyPacket : IGameDataPacket
    {

    }

    [MemoryPackable]
    public sealed partial record LoginPacket(string AccountName, string Password, int ProtocolVersion) : ILobbyPacket;

    [MemoryPackable]
    public sealed partial record RegisterPacket(string AccountName, string Nickname, string Password) : ILobbyPacket;

    [MemoryPackable]
    public sealed partial record RegisterResultPacket(RegistrationStatus Status) : ILobbyPacket;

    [MemoryPackable]
    public sealed partial record ChatPacket(string Message) : ILobbyPacket;

    [MemoryPackable]
    public sealed partial record LoginResultPacket : ILobbyPacket
    {
        [MemoryPackConstructor]
        public LoginResultPacket(LoginStatus status, AccountPacket account, LoginToken token)
        {
            Status = status;
            Account = account;
            Token = token;
        }

        public LoginResultPacket(LoginStatus status)
        {
            Status = status;
        }

        public LoginStatus Status { get; init; }
        public AccountPacket? Account { get; init; }
        public LoginToken Token { get; init; }

        public void Deconstruct(out LoginStatus status, out AccountPacket? account, out LoginToken token)
        {
            status = Status;
            account = Account;
            token = Token;
        }
    }

    [MemoryPackable]
    public sealed partial record PingPacket : ILobbyPacket;

    [MemoryPackable]
    public sealed partial record AccountPacket(string Nickname, string Title, int Wins, int Losses, int Escapes, AvatarShowPacket AvatarShow);

    [MemoryPackable]
    public sealed partial record AvatarShowPacket(int AvatarIndex, int BorderIndex, int BackgroundIndex);

    [MemoryPackable]
    public partial struct LoginToken
    {
        public Guid TokenString { get; init; }
    }

    [MemoryPackable]
    public sealed partial record ServerDisconnectedPacket(DisconnectReason Reason) : ILobbyPacket;

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
