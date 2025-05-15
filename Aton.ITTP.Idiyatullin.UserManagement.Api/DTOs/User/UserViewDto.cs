using Aton.ITTP.Idiyatullin.UserManagement.Api.Entity;

/// <summary>
/// DTO для отображения полной информации о пользователе.
/// </summary>
public class UserViewDto
{
    /// <summary>
    /// Уникальный идентификатор пользователя.
    /// </summary>
    public Guid Guid { get; set; }

    /// <summary>
    /// Логин пользователя.
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Пол пользователя.
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    /// Дата рождения пользователя.
    /// </summary>
    public DateTime? Birthday { get; set; }

    /// <summary>
    /// Является ли пользователь администратором.
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    /// Дата создания пользователя.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Логин пользователя, создавшего этого пользователя.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Дата последнего изменения пользователя.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Логин пользователя, последним изменившего этого пользователя.
    /// </summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Дата блокировки ("мягкого" удаления) пользователя.
    /// </summary>
    public DateTime? RevokedOn { get; set; }

    /// <summary>
    /// Логин пользователя, заблокировавшего этого пользователя.
    /// </summary>
    public string? RevokedBy { get; set; }

    /// <summary>
    /// Активен ли пользователь (не заблокирован).
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Создает экземпляр <see cref="UserViewDto"/> на основе сущности <see cref="User"/>.
    /// </summary>
    /// <param name="user">Сущность пользователя.</param>
    /// <returns>DTO с данными пользователя.</returns>
    public static UserViewDto FromUser(User user)
    {
        return new UserViewDto
        {
            Guid = user.Guid,
            Login = user.Login,
            Name = user.Name,
            Gender = user.Gender,
            Birthday = user.Birthday,
            IsAdmin = user.Admin,
            CreatedOn = user.CreatedOn,
            CreatedBy = user.CreatedBy,
            ModifiedOn = user.ModifiedOn,
            ModifiedBy = user.ModifiedBy,
            RevokedOn = user.RevokedOn,
            RevokedBy = user.RevokedBy,
            IsActive = user.IsActive()
        };
    }
}