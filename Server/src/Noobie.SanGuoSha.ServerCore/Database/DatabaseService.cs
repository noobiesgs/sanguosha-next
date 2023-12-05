using System.Collections.Concurrent;
using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Realms;

namespace Noobie.SanGuoSha.Database;

[RegisterSingleton, AutoConstructor]
public sealed partial class DatabaseService : IDisposable
{
    private Realm? _realm;
    private IQueryable<Account>? _accounts;
    private DatabaseIndexes? _indexes;
    private readonly ILogger<DatabaseService> _logger;
    private readonly object _accountCreationLock = new();
    private readonly ConcurrentDictionary<string, Account> _accountDic = new(StringComparer.OrdinalIgnoreCase);

    public Account? FindAccount(string accountName)
    {
        EnsureDbInitialized();
        if (_accountDic.TryGetValue(accountName, out var account))
        {
            return account;
        }

        account = _accounts!.FirstOrDefault(a => a.AccountName.Equals(accountName, StringComparison.OrdinalIgnoreCase));
        if (account == null)
        {
            return null;
        }

        _accountDic.TryAdd(accountName, account);
        return account;
    }

    public bool TryCreateAccount(Account account)
    {
        EnsureDbInitialized();
        if (_accountDic.ContainsKey(account.AccountName))
        {
            return false;
        }

        lock (_accountCreationLock)
        {
            if (_accountDic.ContainsKey(account.AccountName))
            {
                return false;
            }

            if (_accounts!.Any(a => a.AccountName.Equals(account.AccountName)))
            {
                return false;
            }

            _realm!.Write(() =>
            {
                account.Id = ++_indexes!.AccountIndex;
                _realm.Add(account);
            });

            return _accountDic.TryAdd(account.AccountName, account);
        }
    }

    public void Write(Action action)
    {
        EnsureDbInitialized();
        _realm!.Write(action);
    }

    public bool AccountExist(string accountName)
    {
        EnsureDbInitialized();
        return _accountDic.ContainsKey(accountName) || _accounts!.Any(a => a.AccountName.Equals(accountName, StringComparison.OrdinalIgnoreCase));
    }

    public void Initialize()
    {
        var config = new DbConfigurationBuilder();

        config.SetDbFilePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./sanguosha.account.db"));
        config.WithSchema<DatabaseIndexes>();
        config.WithSchema<Account>();
        config.WithSchema<AvatarShow>();

        _realm = Realm.GetInstance(config.Build());

        _indexes = _realm.All<DatabaseIndexes>().FirstOrDefault();
        if (_indexes == null)
        {
            _indexes = new DatabaseIndexes();
            _realm.Write(() =>
            {
                _realm.Add(_indexes);
            });
        }

        _accounts = _realm.All<Account>();

        _logger.LogInformation("Database initialized");
    }

    private void EnsureDbInitialized()
    {
        if (_accounts == null || _indexes == null || _realm == null)
        {
            throw new InvalidOperationException("The database has not been initialized yet。");
        }
    }

    public void Dispose()
    {
        _realm?.Dispose();
    }
}