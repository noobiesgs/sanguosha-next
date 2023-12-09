using System.Collections.Concurrent;
using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Noobie.SanGuoSha.Network;
using Realms;

namespace Noobie.SanGuoSha.Database;

[RegisterSingleton, AutoConstructor]
public sealed partial class DatabaseService : IDisposable
{
    private readonly ILogger<DatabaseService> _logger;
    private readonly object _accountCreationLock = new();
    private readonly ConcurrentDictionary<string, Account> _accountDic = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Account> _accountNicknameDic = new(StringComparer.OrdinalIgnoreCase);
    private int _accountIndex;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private RealmConfiguration? _configuration;
    private bool _initialized;

    public Account? FindAccount(string accountName)
    {
        if (_accountDic.TryGetValue(accountName, out var account))
        {
            return account;
        }

        return null;
    }

    public RegistrationStatus CreateAccount(Account account)
    {
        if (_accountDic.ContainsKey(account.AccountName))
        {
            return RegistrationStatus.AccountAlreadyExists;
        }

        lock (_accountCreationLock)
        {
            if (_accountDic.ContainsKey(account.AccountName))
            {
                return RegistrationStatus.AccountAlreadyExists;
            }

            if (_accountNicknameDic.ContainsKey(account.Nickname))
            {
                return RegistrationStatus.NicknameAlreadyExists;
            }

            account.Id = Interlocked.Increment(ref _accountIndex);

            if (_accountDic.TryAdd(account.AccountName, account) && _accountNicknameDic.TryAdd(account.Nickname, account))
            {
                return RegistrationStatus.Success;
            }

            return RegistrationStatus.AccountAlreadyExists;
        }
    }

    public bool AccountExist(string accountName)
    {
        return _accountDic.ContainsKey(accountName);
    }

    public void Initialize()
    {
        if (_initialized)
        {
            throw new InvalidOperationException("Db service already initialized");
        }

        _initialized = true;
        var config = new DbConfigurationBuilder();

        config.SetDbFilePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./sanguosha.account.db"));
        config.WithSchema<DatabaseIndexesEntry>();
        config.WithSchema<AccountEntry>();
        config.WithSchema<AvatarShowEntry>();
        _configuration = config.Build();
        using var realm = Realm.GetInstance(_configuration);

        var indexes = realm.All<DatabaseIndexesEntry>().FirstOrDefault();
        if (indexes == null)
        {
            indexes = new DatabaseIndexesEntry();
            realm.Write(() =>
            {
                realm.Add(indexes);
            });
        }

        _accountIndex = indexes.AccountIndex;

        var accounts = realm.All<AccountEntry>();

        foreach (var accountEntry in accounts)
        {
            var account = ObjectMapper.Map<Account>(accountEntry);
            account.IsDirty = false;
            _accountDic.TryAdd(account.AccountName, account);
            _accountNicknameDic.TryAdd(account.Nickname, account);
        }

        _logger.LogInformation("Database initialized");

        Task.Factory.StartNew(SaveLoop, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private async Task SaveLoop()
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));

        while (await timer.WaitForNextTickAsync(_cancellationTokenSource.Token))
        {
            try
            {
                SaveToRealm();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to save data to db");
            }
        }
    }

    private void SaveToRealm()
    {
        using var realm = Realm.GetInstance(_configuration);

        var indexes = realm.All<DatabaseIndexesEntry>().First();

        realm.Write(() =>
        {
            indexes.AccountIndex = _accountIndex;
            foreach (var kvp in _accountDic)
            {
                if (kvp.Value.IsDirty)
                {
                    realm.Add(ObjectMapper.Map<AccountEntry>(kvp.Value), true);
                }
            }
        });

        _logger.LogInformation("Data synced to database");
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        SaveToRealm();
    }
}