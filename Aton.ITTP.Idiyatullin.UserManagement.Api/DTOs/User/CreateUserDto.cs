using Aton.ITTP.Idiyatullin.UserManagement.Api.Constants;
using System.ComponentModel.DataAnnotations;

namespace Aton.ITTP.Idiyatullin.UserManagement.Api.DTOs.User
{
    public class CreateUserDto : UserBaseDto
    {
        /// <summary>
        /// Логин.
        /// </summary>
        [Required(ErrorMessage = "Логин обязателен")]
        [MaxLength(100, ErrorMessage = "Логин не может быть длиннее 100 символов")]
        [RegularExpression(AppConstants.ValidationPatterns.LatinCharsAndDigits, ErrorMessage = "Логин должен содержать только латинские буквы и цифры")]
        public string Login { get; set; } = string.Empty;

        /// <summary>
        /// Пароль.
        /// </summary>
        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(8, ErrorMessage = "Пароль должен быть не менее 8 символов")]
        [RegularExpression(AppConstants.ValidationPatterns.LatinCharsAndDigits, ErrorMessage = "Пароль должен содержать только латинские буквы и цифры")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Является ли администратором.
        /// </summary>
        public bool IsAdmin { get; set; } = false;
    }
}