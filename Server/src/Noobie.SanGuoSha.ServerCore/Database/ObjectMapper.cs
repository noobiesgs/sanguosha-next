using Noobie.SanGuoSha.Network;
using Riok.Mapperly.Abstractions;

namespace Noobie.SanGuoSha.Database
{
    [Mapper]
    internal static partial class ObjectMapper
    {
        public static partial TTarget Map<TTarget>(object source);

        private static partial AccountPacket MapAccount(Account account);

        private static partial Account MapAccountEntry(AccountEntry accountEntry);

        private static partial AccountEntry MapAccountToEntry(Account account);
    }
}
