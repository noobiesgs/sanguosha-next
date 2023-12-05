using Noobie.SanGuoSha.Utils;
using Realms;

namespace Noobie.SanGuoSha.Database;

public partial class Account : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public int Id { get; set; }

    [Indexed]
    public string AccountName { get; set; } = string.Empty;

    public string Password { get; private set; } = string.Empty;

    public byte[] Salt { get; private set; } = Array.Empty<byte>();

    [Indexed]
    public string Nickname { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int Wins { get; set; }

    public int Losses { get; set; }

    public int Escapes { get; set; }

    public bool Banned { get; set; }

    public string BanReason { get; set; } = string.Empty;

    public string? LastIp { get; set; }

    public void SetPassword(string password)
    {
        Salt = Crypto.GenerateSalt();
        Password = Crypto.HashPassword(password, Salt);
    }

    public AvatarShow? AvatarShow { get; set; } = new ();
}

public partial class AvatarShow : IEmbeddedObject
{
    public int AvatarIndex { get; set; }

    public int BorderIndex { get; set; }

    public int BackgroundIndex { get; set; }
}