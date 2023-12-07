using System.ComponentModel;
using System.Runtime.CompilerServices;
using Noobie.SanGuoSha.Utils;

namespace Noobie.SanGuoSha.Database;

public sealed class Account : INotifyPropertyChanged
{
    private int _id;
    private string _accountName = string.Empty;
    private string _password = string.Empty;
    private byte[] _salt = Array.Empty<byte>();
    private string _nickname = string.Empty;
    private string _title = string.Empty;
    private int _wins;
    private int _losses;
    private int _escapes;
    private bool _banned;
    private string _banReason = string.Empty;
    private string? _lastIp;
    private AvatarShow _avatarShow;

    public Account()
    {
        _avatarShow = new AvatarShow();
        _avatarShow.PropertyChanged += AvatarShowOnPropertyChanged;
    }

    public bool IsDirty { get; set; }

    public int Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public string AccountName
    {
        get => _accountName;
        set => SetField(ref _accountName, value);
    }

    public string Password
    {
        get => _password;
        internal set => SetField(ref _password, value);
    }

    public byte[] Salt
    {
        get => _salt;
        internal set => SetField(ref _salt, value);
    }

    public string Nickname
    {
        get => _nickname;
        set => SetField(ref _nickname, value);
    }

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public int Wins
    {
        get => _wins;
        set => SetField(ref _wins, value);
    }

    public int Losses
    {
        get => _losses;
        set => SetField(ref _losses, value);
    }

    public int Escapes
    {
        get => _escapes;
        set => SetField(ref _escapes, value);
    }

    public bool Banned
    {
        get => _banned;
        set => SetField(ref _banned, value);
    }

    public string BanReason
    {
        get => _banReason;
        set => SetField(ref _banReason, value);
    }

    public string? LastIp
    {
        get => _lastIp;
        set => SetField(ref _lastIp, value);
    }

    public void SetPassword(string password)
    {
        Salt = Crypto.GenerateSalt();
        Password = Crypto.HashPassword(password, Salt);
    }

    public AvatarShow AvatarShow
    {
        get => _avatarShow;
        set
        {
            if (value == _avatarShow)
            {
                return;
            }

            _avatarShow.PropertyChanged -= AvatarShowOnPropertyChanged;
            SetField(ref _avatarShow, value);
            _avatarShow.PropertyChanged += AvatarShowOnPropertyChanged;
        }
    }

    private void AvatarShowOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        IsDirty = true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        IsDirty = true;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

public class AvatarShow : INotifyPropertyChanged
{
    private int _backgroundIndex;
    private int _borderIndex;
    private int _avatarIndex;

    public int AvatarIndex
    {
        get => _avatarIndex;
        set => SetField(ref _avatarIndex, value);
    }

    public int BorderIndex
    {
        get => _borderIndex;
        set => SetField(ref _borderIndex, value);
    }

    public int BackgroundIndex
    {
        get => _backgroundIndex;
        set => SetField(ref _backgroundIndex, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}