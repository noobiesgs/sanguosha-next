using System.Collections.Concurrent;
using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Noobie.SanGuoSha.Database;
using Noobie.SanGuoSha.Network;
using Noobie.SanGuoSha.Utils;

namespace Noobie.SanGuoSha.Lobby;

[RegisterSingleton, AutoConstructor]
public partial class LobbyService
{
    private readonly DatabaseService _dbService;
    private readonly ConcurrentDictionary<string, ServerPlayer> _loggedInPlayersConnectionIdMap = new();
    private readonly ConcurrentDictionary<string, ServerPlayer> _loggedInPlayersAccountMap = new();
    private readonly ILogger<LobbyService> _logger;

    public bool Register(string accountName, string nickname, string password)
    {
        if (!_dbService.AccountExist(accountName))
        {
            var account = new Account
            {
                AccountName = accountName,
                Nickname = nickname
            };
            account.SetPassword(password);
            return _dbService.TryCreateAccount(account);
        }

        return false;
    }

    //TODO
    public LoginStatus Login(SanGuoShaTcpClient connection, int version, string accountName, string password, out Account? account, out LoginToken reconnectionToken)
    {
        reconnectionToken = new LoginToken();
        account = null;

        if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(password))
        {
            return LoginStatus.InvalidUserNameAndPassword;
        }

        if (version != Misc.ProtocolVersion)
        {
            _logger.LogWarning("Client login failed with outdated protocol version: {version}, ip: {ip}", version, connection.Ip);
            return LoginStatus.OutdatedVersion;
        }

        account = Authenticate(accountName, password);

        if (account == null)
        {
            return LoginStatus.InvalidUserNameAndPassword;
        }

        lock (_loggedInPlayersAccountMap)
        {
            if (_loggedInPlayersAccountMap.TryGetValue(account.AccountName, out var player))
            {
                if (player.Connection != null)
                {
                    if (player.Connection == connection)
                    {
                        _logger.LogWarning("player logged in twice? Connection ip: {ip}", player.Connection.Ip);
                        return LoginStatus.UnknownFailure;
                    }

                    _logger.LogWarning("player already logged in, kick out. Old connection ip: {ip}", player.Connection.Ip);
                    _loggedInPlayersConnectionIdMap.TryRemove(player.Connection.Id, out _);
                    player.Connection.Send(new ServerDisconnectedPacket(DisconnectReason.LoggedInOnAnotherDevice));
                    player.Connection.Close(nameof(DisconnectReason.LoggedInOnAnotherDevice));
                }

                player.Connection = connection;
                reconnectionToken = player.LoginToken;
                _loggedInPlayersConnectionIdMap.TryAdd(connection.Id, player);
            }
            else
            {
                player = new ServerPlayer(account)
                {
                    LoginToken = reconnectionToken,
                    Connection = connection
                };
                _loggedInPlayersAccountMap.TryAdd(account.AccountName, player);
                _loggedInPlayersConnectionIdMap.TryAdd(connection.Id, player);
            }

            player.Account.LastIp = connection.Ip;
        }
        _logger.LogWarning("player logged in. Ip: {ip}", connection.Ip);
        return LoginStatus.Success;
    }

    private Account? Authenticate(string accountName, string password)
    {
        var account = _dbService.FindAccount(accountName);
        if (account is not null)
        {
            var hash = Crypto.HashPassword(password, account.Salt);
            if (hash == account.Password)
            {
                return account;
            }

            return null;
        }

        return null;
    }

    public bool IsLoggedId(string connectId)
    {
        return _loggedInPlayersConnectionIdMap.ContainsKey(connectId);
    }

    public void ClientDisconnected(SanGuoShaTcpClient connection)
    {
        //TODO:
        if (_loggedInPlayersConnectionIdMap.TryRemove(connection.Id, out var player))
        {
            _loggedInPlayersAccountMap.TryRemove(player.Account.AccountName, out _);
        }
    }
}