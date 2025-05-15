namespace Aton.ITTP.Idiyatullin.UserManagement.Api.Helpers
{
    /// <summary>
    /// Хеширование и проверка паролей с использованием BCrypt.
    /// </summary>
    public struct PasswordHasher
    {
        /// <summary>
        /// Хеширует указанный пароль.
        /// </summary>
        /// <param name="password">Пароль для хеширования.</param>
        /// <returns>Хеш пароля в виде строки.</returns>
        public static string HashPassword(string password)
        {
            ValidationHelper.IsStringNullOrEmpty(password);

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Проверяет, соответствует ли указанный пароль хешированному паролю.
        /// </summary>
        /// <param name="password">Пароль для проверки (в открытом виде).</param>
        /// <param name="hashedPassword">Хешированный пароль для сравнения.</param>
        /// <returns>True, если пароль совпадает с хешем, иначе false.</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            ValidationHelper.IsStringNullOrEmpty(password);
            ValidationHelper.IsStringNullOrEmpty(hashedPassword);

            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}