using System.Collections.Concurrent;
using System.Text.RegularExpressions;
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

    [GeneratedRegex(Misc.AccountNamePattern)]
    private partial Regex AccountNameRegex();

    [GeneratedRegex(Misc.NicknamePattern)]
    private partial Regex NicknameRegex();

    [GeneratedRegex(Misc.PasswordPattern)]
    private partial Regex PasswordRegex();

    public RegistrationStatus Register(string accountName, string nickname, string password)
    {
        if (!AccountNameRegex().IsMatch(accountName)
            || !NicknameRegex().IsMatch(nickname)
            || !PasswordRegex().IsMatch(password))
        {
            _logger.LogInformation("Invalid registration info: {account}, {nickname}, {password}", accountName, nickname, password);
            return RegistrationStatus.Invalid;
        }

        var account = new Account
        {
            AccountName = accountName,
            Nickname = nickname
        };
        account.SetPassword(password);
        return _dbService.CreateAccount(account);
    }

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
                        _logger.LogWarning("player logged in twice? Connection ip: {ip}, account: {accountName}", player.Connection.Ip, account.AccountName);
                        return LoginStatus.UnknownFailure;
                    }

                    _logger.LogWarning("player already logged in, kick out. Old connection ip: {ip}, account: {accountName}", player.Connection.Ip, account.AccountName);
                    _loggedInPlayersConnectionIdMap.TryRemove(player.Connection.Id, out _);
                    player.Connection.Send(new ServerDisconnectedPacket(DisconnectReason.LoggedInOnAnotherDevice));
                    player.Connection.Close(nameof(DisconnectReason.LoggedInOnAnotherDevice));
                }

                player.Connection = connection;
                reconnectionToken = player.LoginToken;
                _loggedInPlayersConnectionIdMap.TryAdd(connection.Id, player);
            }
            else if (_loggedInPlayersConnectionIdMap.TryGetValue(connection.Id, out player))
            {
                _logger.LogWarning("player already logged in with another account? Connection ip: {ip}, account: {accountName}, exists account: {existsAccount}",
                    player.Connection!.Ip, account.AccountName, player.Account.AccountName);
                return LoginStatus.UnknownFailure;
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
        _logger.LogInformation("player logged in. Ip: {ip}, account: {accountName}", connection.Ip, account.AccountName);
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
        //TODO: 正在游戏的玩家需要断线重连逻辑
        if (_loggedInPlayersConnectionIdMap.TryRemove(connection.Id, out var player))
        {
            _loggedInPlayersAccountMap.TryRemove(player.Account.AccountName, out _);
        }
    }
}