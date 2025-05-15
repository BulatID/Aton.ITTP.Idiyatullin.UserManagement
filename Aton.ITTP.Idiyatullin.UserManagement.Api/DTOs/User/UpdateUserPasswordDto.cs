using Aton.ITTP.Idiyatullin.UserManagement.Api.Constants;
using System.ComponentModel.DataAnnotations;

namespace Aton.ITTP.Idiyatullin.UserManagement.Api.DTOs.User
{
    public class UpdateUserPasswordDto
    {
        [Required(ErrorMessage = "Новый пароль обязателен")]
        [MinLength(8, ErrorMessage = "Пароль должен быть не менее 8 символов")]
        [RegularExpression(AppConstants.ValidationPatterns.LatinCharsAndDigits, ErrorMessage = "Пароль должен содержать только латинские буквы и цифры")]
        public string NewPassword { get; set; } = string.Empty;
    }
}