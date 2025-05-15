using Aton.ITTP.Idiyatullin.UserManagement.Api.Data;
using Aton.ITTP.Idiyatullin.UserManagement.Api.DTOs.User;
using Aton.ITTP.Idiyatullin.UserManagement.Api.Entity;
using Aton.ITTP.Idiyatullin.UserManagement.Api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Aton.ITTP.Idiyatullin.UserManagement.Api.Services
{
    /// <summary>
    /// Представляет результат выполнения операции сервиса, который может содержать данные.
    /// </summary>
    /// <typeparam name="T">Тип данных, возвращаемых в случае успеха.</typeparam>
    public class UserService : IUserService
    {
        /// <summary>
        /// Контекст базы данных.
        /// </summary>
        private readonly UserManagementDbContext _context;

        /// <summary>
        /// Логгер.
        /// </summary>
        private readonly ILogger<UserService> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="UserService"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных.</param>
        /// <param name="logger">Логгер.</param>
        public UserService(UserManagementDbContext context, ILogger<UserService> logger)
        {
            ValidationHelper.IsNull(_context = context);
            ValidationHelper.IsNull(_logger = logger);
        }

        /// <summary>
        /// Асинхронно получает пользователя, выполняющего действие (actor), по его логину.
        /// Пользователь должен быть активен (не заблокирован).
        /// </summary>
        /// <param name="actorLogin">Логин пользователя, выполняющего действие.</param>
        /// <returns>
        /// Сущность <see cref="User"/>, если пользователь найден и активен; иначе <c>null</c>.
        /// </returns>
        private async Task<User?> GetActorAsync(string actorLogin)
        {
            if (string.IsNullOrWhiteSpace(actorLogin))
            {
                return null;
            }

            return await _context.Users.FirstOrDefaultAsync(u => u.Login == actorLogin && u.RevokedOn == null);
        }

        /// <summary>
        /// Создает нового пользователя в системе.
        /// </summary>
        /// <param name="createUserDto">Данные для создания пользователя.</param>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратора).</param>
        /// <returns>Результат операции, содержащий DTO созданного пользователя или ошибку.</returns>
        public async Task<ServiceResult<UserViewDto>> CreateUserAsync(CreateUserDto createUserDto, string actorLogin)
        {
            var actor = await GetActorAsync(actorLogin);

            if (actor == null || !actor.Admin)
            {
                return ServiceResult<UserViewDto>.Failure("Только администратор может создавать пользователей.", 403);
            }

            if (await _context.Users.AnyAsync(u => u.Login == createUserDto.Login))
            {
                return ServiceResult<UserViewDto>.Failure($"Пользователь с логином '{createUserDto.Login}' уже существует.", 409);
            }

            var user = new User
            {
                Guid = Guid.NewGuid(),
                Login = createUserDto.Login,
                PasswordHash = PasswordHasher.HashPassword(createUserDto.Password),
                Name = createUserDto.Name,
                Gender = createUserDto.Gender,
                Birthday = createUserDto.Birthday,
                Admin = createUserDto.IsAdmin,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = actor.Login
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Login} created by {ActorLogin}", user.Login, actorLogin);

            return ServiceResult<UserViewDto>.Success(UserViewDto.FromUser(user), 201);
        }

        /// <summary>
        /// Обновляет персональную информацию указанного пользователя (имя, пол, дата рождения).
        /// </summary>
        /// <param name="targetUserLogin">Логин пользователя, чьи данные обновляются.</param>
        /// <param name="updateUserDto">Новые данные пользователя.</param>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратор или сам пользователь).</param>
        /// <returns>Результат операции, содержащий обновленный DTO пользователя или ошибку.</returns>
        public async Task<ServiceResult<UserViewDto>> UpdateUserPersonalInfoAsync(string targetUserLogin, UpdateUserPersonalInfoDto updateUserDto, string actorLogin)
        {
            var actor = await GetActorAsync(actorLogin);

            if (actor == null)
            {
                return ServiceResult<UserViewDto>.Failure("Действующий пользователь не найден или не активен.", 401);
            }

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == targetUserLogin);

            if (targetUser == null)
            {
                return ServiceResult<UserViewDto>.Failure("Пользователь для обновления не найден.", 404);
            }

            var canUpdate = actor.Admin || (actor.Login == targetUser.Login && targetUser.IsActive());

            if (!canUpdate)
            {
                return ServiceResult<UserViewDto>.Failure("Недостаточно прав для изменения данных пользователя.", 403);
            }

            if (!targetUser.IsActive() && actor.Login == targetUser.Login && !actor.Admin)
            {
                return ServiceResult<UserViewDto>.Failure("Неактивный пользователь не может изменять свои данные.", 403);
            }

            targetUser.Name = updateUserDto.Name;
            targetUser.Gender = updateUserDto.Gender;
            targetUser.Birthday = updateUserDto.Birthday;
            targetUser.ModifiedOn = DateTime.UtcNow;
            targetUser.ModifiedBy = actor.Login;

            await _context.SaveChangesAsync();
            _logger.LogInformation("User {Login} personal info updated by {ActorLogin}", targetUser.Login, actorLogin);

            return ServiceResult<UserViewDto>.Success(UserViewDto.FromUser(targetUser));
        }

        /// <summary>
        /// Обновляет пароль указанного пользователя.
        /// </summary>
        /// <param name="targetUserLogin">Логин пользователя, чей пароль обновляется.</param>
        /// <param name="updatePasswordDto">Данные для обновления пароля.</param>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратор или сам пользователь).</param>
        /// <returns>Результат операции (успех/ошибка).</returns>
        public async Task<ServiceResult> UpdateUserPasswordAsync(string targetUserLogin, UpdateUserPasswordDto updatePasswordDto, string actorLogin)
        {
            var actor = await GetActorAsync(actorLogin);

            if (actor == null)
            {
                return ServiceResult.Failure("Действующий пользователь не найден или не активен.", 401);
            }

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == targetUserLogin);

            if (targetUser == null)
            {
                return ServiceResult.Failure("Пользователь для обновления пароля не найден.", 404);
            }

            var canUpdate = actor.Admin || (actor.Login == targetUser.Login && targetUser.IsActive());

            if (!canUpdate)
            {
                return ServiceResult.Failure("Недостаточно прав для изменения пароля.", 403);
            }

            if (!targetUser.IsActive() && actor.Login == targetUser.Login && !actor.Admin)
            {
                return ServiceResult.Failure("Неактивный пользователь не может изменять свой пароль.", 403);
            }

            targetUser.PasswordHash = PasswordHasher.HashPassword(updatePasswordDto.NewPassword);
            targetUser.ModifiedOn = DateTime.UtcNow;
            targetUser.ModifiedBy = actor.Login;

            await _context.SaveChangesAsync();
            _logger.LogInformation("User {Login} password updated by {ActorLogin}", targetUser.Login, actorLogin);

            return ServiceResult.Success(200);
        }

        /// <summary>
        /// Обновляет логин указанного пользователя. Логин должен оставаться уникальным.
        /// </summary>
        /// <param name="targetUserLogin">Текущий логин пользователя, чей логин обновляется.</param>
        /// <param name="updateLoginDto">Данные для обновления логина.</param>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратор или сам пользователь).</param>
        /// <returns>Результат операции, содержащий обновленный DTO пользователя (с новым логином) или ошибку.</returns>
        public async Task<ServiceResult<UserViewDto>> UpdateUserLoginAsync(string targetUserLogin, UpdateUserLoginDto updateLoginDto, string actorLogin)
        {
            var actor = await GetActorAsync(actorLogin);

            if (actor == null)
            {
                return ServiceResult<UserViewDto>.Failure("Действующий пользователь не найден или не активен.", 401);
            }

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == targetUserLogin);

            if (targetUser == null)
            {
                return ServiceResult<UserViewDto>.Failure("Пользователь для обновления логина не найден.", 404);
            }

            var canUpdate = actor.Admin || (actor.Login == targetUser.Login && targetUser.IsActive());

            if (!canUpdate)
            {
                return ServiceResult<UserViewDto>.Failure("Недостаточно прав для изменения логина.", 403);
            }

            if (!targetUser.IsActive() && actor.Login == targetUser.Login && !actor.Admin)
            {
                return ServiceResult<UserViewDto>.Failure("Неактивный пользователь не может изменять свой логин.", 403);
            }


            if (targetUser.Login != updateLoginDto.NewLogin && await _context.Users.AnyAsync(u => u.Login == updateLoginDto.NewLogin))
            {
                return ServiceResult<UserViewDto>.Failure($"Логин '{updateLoginDto.NewLogin}' уже занят.", 409);
            }

            targetUser.Login = updateLoginDto.NewLogin;
            targetUser.ModifiedOn = DateTime.UtcNow;
            targetUser.ModifiedBy = actor.Login;

            await _context.SaveChangesAsync();
            _logger.LogInformation("User {OldLogin} login changed to {NewLogin} by {ActorLogin}", targetUserLogin, targetUser.Login, actorLogin);

            return ServiceResult<UserViewDto>.Success(UserViewDto.FromUser(targetUser));
        }

        /// <summary>
        /// Получает список всех активных (не заблокированных) пользователей, отсортированных по дате создания.
        /// </summary>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратора).</param>
        /// <returns>Результат операции, содержащий коллекцию DTO активных пользователей или ошибку.</returns>
        public async Task<ServiceResult<IEnumerable<UserViewDto>>> GetActiveUsersAsync(string actorLogin)
        {
            var actor = await GetActorAsync(actorLogin);

            if (actor == null || !actor.Admin)
            {
                return ServiceResult<IEnumerable<UserViewDto>>.Failure("Только администратор может просматривать список всех активных пользователей.", 403);
            }

            var users = await _context.Users
                .Where(u => u.RevokedOn == null)
                .OrderBy(u => u.CreatedOn)
                .Select(u => UserViewDto.FromUser(u))
                .ToListAsync();

            return ServiceResult<IEnumerable<UserViewDto>>.Success(users);
        }

        /// <summary>
        /// Получает краткую информацию (имя, пол, дата рождения, статус) о пользователе по его логину.
        /// </summary>
        /// <param name="targetUserLogin">Логин искомого пользователя.</param>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратора).</param>
        /// <returns>Результат операции, содержащий DTO с краткой информацией о пользователе или ошибку.</returns>
        public async Task<ServiceResult<UserBriefViewDto>> GetUserByLoginAsync(string targetUserLogin, string actorLogin)
        {
            var actor = await GetActorAsync(actorLogin);

            if (actor == null || !actor.Admin)
            {
                return ServiceResult<UserBriefViewDto>.Failure("Только администратор может запрашивать данные пользователя по логину.", 403);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == targetUserLogin);

            if (user == null)
            {
                return ServiceResult<UserBriefViewDto>.Failure("Пользователь не найден.", 404);
            }

            return ServiceResult<UserBriefViewDto>.Success(UserBriefViewDto.FromUser(user));
        }

        /// <summary>
        /// Получает полную информацию о пользователе для самого пользователя на основе логина и пароля.
        /// Используется для самопроверки данных пользователем.
        /// </summary>
        /// <param name="targetUserLogin">Логин пользователя.</param>
        /// <param name="password">Пароль пользователя.</param>
        /// <returns>Результат операции, содержащий DTO пользователя или ошибку (например, при неверном пароле).</returns>
        public async Task<ServiceResult<UserViewDto>> GetUserForSelfAsync(string targetUserLogin, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == targetUserLogin);

            if (user == null || !user.IsActive())
            {
                return ServiceResult<UserViewDto>.Failure("Пользователь не найден или не активен.", 404);
            }

            if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                return ServiceResult<UserViewDto>.Failure("Неверный логин или пароль.", 401);
            }

            return ServiceResult<UserViewDto>.Success(UserViewDto.FromUser(user));
        }

        /// <summary>
        /// Получает список всех пользователей, которые старше указанного возраста.
        /// </summary>
        /// <param name="age">Возраст, старше которого должны быть пользователи.</param>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратора).</param>
        /// <returns>Результат операции, содержащий коллекцию DTO пользователей старше заданного возраста или ошибку.</returns>
        public async Task<ServiceResult<IEnumerable<UserViewDto>>> GetUsersOlderThanAsync(int age, string actorLogin)
        {
            var actor = await GetActorAsync(actorLogin);

            if (actor == null || !actor.Admin)
            {
                return ServiceResult<IEnumerable<UserViewDto>>.Failure("Только администратор может выполнять этот запрос.", 403);
            }

            if (age < 0)
            {
                return ServiceResult<IEnumerable<UserViewDto>>.Failure("Возраст не может быть отрицательным.", 400);
            }

            return ServiceResult<IEnumerable<UserViewDto>>.Success(
                await _context.Users
                    .Where(u => u.Birthday != null && u.Birthday.Value.Date <= DateTime.UtcNow.AddYears(-age).Date)
                    .OrderBy(u => u.Birthday)
                    .Select(u => UserViewDto.FromUser(u))
                    .ToListAsync());
        }

        /// <summary>
        /// Удаляет пользователя. Удаление может быть "мягким" (установка флагов RevokedOn, RevokedBy) или полным.
        /// </summary>
        /// <param name="targetUserLogin">Логин пользователя для удаления.</param>
        /// <param name="hardDelete">Флаг, указывающий на тип удаления: true - полное удаление, false - "мягкое" удаление.</param>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратора).</param>
        /// <returns>Результат операции (успех/ошибка).</returns>
        public async Task<ServiceResult> DeleteUserAsync(string targetUserLogin, bool hardDelete, string actorLogin)
        {
            var actor = await GetActorAsync(actorLogin);

            if (actor == null || !actor.Admin)
            {
                return ServiceResult.Failure("Только администратор может удалять пользователей.", 403);
            }

            if (actor.Login == targetUserLogin)
            {
                return ServiceResult.Failure("Администратор не может удалить сам себя.", 403);
            }

            var userToDelete = await _context.Users.FirstOrDefaultAsync(u => u.Login == targetUserLogin);

            if (userToDelete == null)
            {
                return ServiceResult.Failure("Пользователь для удаления не найден.", 404);
            }

            if (hardDelete)
            {
                _context.Users.Remove(userToDelete);
                _logger.LogInformation("User {Login} hard deleted by {ActorLogin}", userToDelete.Login, actorLogin);
            }
            else
            {
                if (userToDelete.IsActive())
                {
                    userToDelete.RevokedOn = DateTime.UtcNow;
                    userToDelete.RevokedBy = actor.Login;
                    userToDelete.ModifiedOn = DateTime.UtcNow;
                    userToDelete.ModifiedBy = actor.Login;
                    _logger.LogInformation("User {Login} soft deleted by {ActorLogin}", userToDelete.Login, actorLogin);
                }
                else
                {
                    _logger.LogInformation("User {Login} was already soft deleted. No action taken by {ActorLogin}.", userToDelete.Login, actorLogin);
                }
            }

            await _context.SaveChangesAsync();

            return ServiceResult.Success(hardDelete 
                ? 204 
                : 200);
        }

        /// <summary>
        /// Восстанавливает "мягко" удаленного пользователя (очищает поля RevokedOn, RevokedBy).
        /// </summary>
        /// <param name="targetUserLogin">Логин пользователя для восстановления.</param>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратора).</param>
        /// <returns>Результат операции, содержащий DTO восстановленного пользователя или ошибку.</returns>
        public async Task<ServiceResult<UserViewDto>> RestoreUserAsync(string targetUserLogin, string actorLogin)
        {
            var actor = await GetActorAsync(actorLogin);

            if (actor == null || !actor.Admin)
            {
                return ServiceResult<UserViewDto>.Failure("Только администратор может восстанавливать пользователей.", 403);
            }

            var userToRestore = await _context.Users.FirstOrDefaultAsync(u => u.Login == targetUserLogin);

            if (userToRestore == null)
            {
                return ServiceResult<UserViewDto>.Failure("Пользователь для восстановления не найден.", 404);
            }

            if (userToRestore.IsActive())
            {
                _logger.LogInformation("User {Login} is already active. No restoration needed by {ActorLogin}.", userToRestore.Login, actorLogin);
                return ServiceResult<UserViewDto>.Success(UserViewDto.FromUser(userToRestore));
            }

            userToRestore.RevokedOn = null;
            userToRestore.RevokedBy = null;
            userToRestore.ModifiedOn = DateTime.UtcNow;
            userToRestore.ModifiedBy = actor.Login;

            await _context.SaveChangesAsync();
            _logger.LogInformation("User {Login} restored by {ActorLogin}", userToRestore.Login, actorLogin);

            return ServiceResult<UserViewDto>.Success(UserViewDto.FromUser(userToRestore));
        }
    }
}