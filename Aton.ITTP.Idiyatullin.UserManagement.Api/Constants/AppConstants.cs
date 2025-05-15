namespace Aton.ITTP.Idiyatullin.UserManagement.Api.Constants
{
    public static class AppConstants
    {
        public const string SystemUserLogin = "SYSTEM";
        public const string DefaultAdminLoginConfigKey = "InitialAdmin:Login";
        public const string DefaultAdminPasswordConfigKey = "InitialAdmin:Password";
        public const string DefaultAdminNameConfigKey = "InitialAdmin:Name";

        public static class ValidationPatterns
        {
            public const string LatinCharsAndDigits = "^[a-zA-Z0-9]+$";
            public const string LatinAndRussianChars = "^[a-zA-Zа-яА-ЯёЁ]+$";
        }
    }
}