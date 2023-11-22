namespace Noobie.SanGuoSha;

public class ServerOptions
{
    public const string Section = "Server";

    /// <summary>
    /// Default value: 0.0.0.0
    /// </summary>
    public string ServerIp { get; set; } = "0.0.0.0";

    /// <summary>
    /// Default value: 7000
    /// </summary>
    public int ServerPort { get; set; } = 7000;
}