using Aton.ITTP.Idiyatullin.UserManagement.Api.Entity;

/// <summary>
/// DTO для отображения краткой информации о пользователе (имя, пол, дата рождения, статус активности).
/// </summary>
public class UserBriefViewDto
{
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
    /// Активен ли пользователь.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Создает экземпляр <see cref="UserBriefViewDto"/> на основе сущности <see cref="User"/>.
    /// </summary>
    /// <param name="user">Сущность пользователя.</param>
    /// <returns>DTO с краткой информацией о пользователе.</returns>
    public static UserBriefViewDto FromUser(User user)
    {
        return new UserBriefViewDto
        {
            Name = user.Name,
            Gender = user.Gender,
            Birthday = user.Birthday,
            IsActive = user.IsActive()
        };
    }
}