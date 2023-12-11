using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby
{
    public class Account
    {
        public static readonly Account Empty = new();

        public string Nickname { get; private set; }

        public string Title { get; private set; }

        public int Wins { get; private set; }

        public int Losses { get; private set; }

        public int Escapes { get; private set; }

        public AvatarShow AvatarShow { get; } = new();

        public void Update(AccountPacket packet)
        {
            Nickname = packet.Nickname;
            Title = packet.Title;
            Wins = packet.Wins;
            Losses = packet.Losses;
            Escapes = packet.Escapes;
            AvatarShow.Update(packet.AvatarShow);
        }
    }

    public class AvatarShow
    {
        public int AvatarIndex { get; private set; }
        public int BorderIndex { get; private set; }
        public int BackgroundIndex { get; private set; }

        public void Update(AvatarShowPacket packet)
        {
            AvatarIndex = packet.AvatarIndex;
            BorderIndex = packet.BorderIndex;
            BackgroundIndex = packet.BackgroundIndex;
        }
    }
}
