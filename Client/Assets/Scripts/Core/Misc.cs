namespace Noobie.SanGuoSha
{
    public static class Misc
    {
        public const int ProtocolVersion = 1;
        public const int MaxChatLength = 160;

        public const string AccountNamePattern = "^[a-zA-Z0-9_]{5,20}$";
        public const string NicknamePattern = @"^[\u4e00-\u9fa5a-zA-Z0-9]{2,7}$";
        public const string PasswordPattern = @"^[a-zA-Z0-9!@#$%^&*()_+{}|:""<>?`\-=[\]';,/\\.]{5,20}$";
    }
}