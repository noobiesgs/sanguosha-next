using Realms;
using System.Diagnostics.CodeAnalysis;

namespace Noobie.SanGuoSha.Database;

internal class DbConfigurationBuilder
{
    private readonly List<Type> _schemaTypes = new();

    private string? _dbFilePath;

    public DbConfigurationBuilder WithSchema<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
    {
        _schemaTypes.Add(typeof(T));
        return this;
    }

    public DbConfigurationBuilder SetDbFilePath(string dbFilePath)
    {
        _dbFilePath = dbFilePath;
        return this;
    }

    public RealmConfiguration Build()
    {
        var config = new RealmConfiguration(_dbFilePath)
        {
            Schema = _schemaTypes
        };

        return config;
    }
}