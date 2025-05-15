using System.ComponentModel.DataAnnotations;

namespace Aton.ITTP.Idiyatullin.UserManagement.Api.Entity
{
    public class User
    {
        [Key]
        public Guid Guid { get; set; }

        [Required]
        [MaxLength(100)]
        public string Login { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public Gender Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public bool Admin { get; set; }

        public DateTime CreatedOn { get; set; }
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        [MaxLength(100)]
        public string? ModifiedBy { get; set; }
        public DateTime? RevokedOn { get; set; }
        [MaxLength(100)]
        public string? RevokedBy { get; set; }

        public bool IsActive() => RevokedOn == null;
    }
}