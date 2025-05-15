using Aton.ITTP.Idiyatullin.UserManagement.Api.Constants;
using Aton.ITTP.Idiyatullin.UserManagement.Api.Entity;
using System.ComponentModel.DataAnnotations;

namespace Aton.ITTP.Idiyatullin.UserManagement.Api.DTOs.User
{
    public class UserBaseDto
    {
        /// <summary>
        /// Имя.
        /// </summary>
        [Required(ErrorMessage = "Имя обязательно")]
        [MaxLength(100, ErrorMessage = "Имя не может быть длиннее 100 символов")]
        [RegularExpression(AppConstants.ValidationPatterns.LatinAndRussianChars, ErrorMessage = "Имя должно содержать только латинские или русские буквы")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Пол.
        /// </summary>
        [Required(ErrorMessage = "Пол обязателен")]
        public Gender Gender { get; set; }

        /// <summary>
        /// Дата рождения.
        /// </summary>
        public DateTime? Birthday { get; set; }
    }
}