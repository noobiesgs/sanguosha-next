using System.Runtime.CompilerServices;

namespace UnityEngine.Localization.Tables
{
    internal static class StringTableExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string L(this StringTable table, string key)
        {
            return table[key]?.GetLocalizedString() ?? key;
        }
    }
}
