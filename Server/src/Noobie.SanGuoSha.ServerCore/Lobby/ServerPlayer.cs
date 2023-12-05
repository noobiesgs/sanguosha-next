using Noobie.SanGuoSha.Database;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby;

internal class ServerPlayer
{
    public ServerPlayer(Account account)
    {
        Account = account;
    }

    public Account Account { get; }

    public SanGuoShaTcpClient? Connection { get; set; }

    public LoginToken LoginToken { get; set; }

    public Room? Room { get; set; }
}