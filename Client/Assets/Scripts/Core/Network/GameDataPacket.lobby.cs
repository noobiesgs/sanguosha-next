#nullable enable
using System;
using MemoryPack;

namespace Noobie.SanGuoSha.Network
{
    [MemoryPackable]
    [MemoryPackUnion(30001, typeof(LoginPacket))]
    [MemoryPackUnion(30002, typeof(RegisterPacket))]
    [MemoryPackUnion(30003, typeof(ChatPacket))]
    [MemoryPackUnion(30004, typeof(LoginResultPacket))]
    [MemoryPackUnion(30005, typeof(ServerDisconnectedPacket))]
    [MemoryPackUnion(30006, typeof(RegisterResultPacket))]
    [MemoryPackUnion(30007, typeof(PingPacket))]
    public abstract partial record LobbyPacket : GameDataPacket;

    [MemoryPackable]
    public sealed partial record LoginPacket(string AccountName, string Password, int ProtocolVersion) : LobbyPacket;

    [MemoryPackable]
    public sealed partial record RegisterPacket(string AccountName, string Password, string Nickname) : LobbyPacket;

    [MemoryPackable]
    public sealed partial record RegisterResultPacket(RegisterStatus Status) : LobbyPacket;

    [MemoryPackable]
    public sealed partial record ChatPacket(string Message) : LobbyPacket;

    [MemoryPackable]
    public sealed partial record LoginResultPacket : LobbyPacket
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
    public sealed partial record PingPacket : LobbyPacket;

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
    public sealed partial record ServerDisconnectedPacket(DisconnectReason Reason) : LobbyPacket;

    public enum DisconnectReason
    {
        LoggedInOnAnotherDevice
    }

    public enum RegisterStatus : byte
    {
        Success,
        AccountAlreadyExists,
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
