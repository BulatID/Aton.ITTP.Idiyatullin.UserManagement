using Aton.ITTP.Idiyatullin.UserManagement.Api.Constants;
using System.ComponentModel.DataAnnotations;

namespace Aton.ITTP.Idiyatullin.UserManagement.Api.DTOs.User
{
    public class UpdateUserLoginDto
    {
        /// <summary>
        /// Новый логин.
        /// </summary>
        [Required(ErrorMessage = "Новый логин обязателен")]
        [MaxLength(100, ErrorMessage = "Логин не может быть длиннее 100 символов")]
        [RegularExpression(AppConstants.ValidationPatterns.LatinCharsAndDigits, ErrorMessage = "Новый логин должен содержать только латинские буквы и цифры")]
        public string NewLogin { get; set; } = string.Empty;
    }
}