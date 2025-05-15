using Aton.ITTP.Idiyatullin.UserManagement.Api.Constants;
using System.ComponentModel.DataAnnotations;

namespace Aton.ITTP.Idiyatullin.UserManagement.Api.DTOs
{
    /// <summary>
    /// DTO для запроса аутентификации пользователя по логину и паролю (для эндпоинта самопроверки).
    /// </summary>
    public class AuthenticateRequestDto
    {
        /// <summary>
        /// Логин пользователя.
        /// </summary>
        [Required]
        [RegularExpression(AppConstants.ValidationPatterns.LatinCharsAndDigits)]
        public string Login { get; set; } = string.Empty;

        /// <summary>
        /// Пароль пользователя.
        /// </summary>
        [Required]
        [RegularExpression(AppConstants.ValidationPatterns.LatinCharsAndDigits)]
        public string Password { get; set; } = string.Empty;
    }
}