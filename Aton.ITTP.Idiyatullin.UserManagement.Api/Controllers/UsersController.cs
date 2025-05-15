using Aton.ITTP.Idiyatullin.UserManagement.Api.DTOs.User;
using Aton.ITTP.Idiyatullin.UserManagement.Api.DTOs;
using Aton.ITTP.Idiyatullin.UserManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aton.ITTP.Idiyatullin.UserManagement.Api.Controllers
{
    /// <summary>
    /// Контроллер для управления пользователями (Users).
    /// Предоставляет CRUD операции и другие действия над сущностью User.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// Работа с пользователями.
        /// </summary>
        private readonly IUserService _userService;

        /// <summary>
        /// Логгер.
        /// </summary>
        private readonly ILogger<UsersController> _logger;

        /// <summary>
        /// Получает логин действующего лица.
        /// </summary>
        /// <returns>Логин.</returns>
        private string GetActorLogin() 
            => HttpContext.Request.Headers["X-Acting-User-Login"].FirstOrDefault() ?? string.Empty;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="UsersController"/>.
        /// </summary>
        /// <param name="userService">Сервис для работы с пользователями.</param>
        /// <param name="logger">Логгер.</param>
        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Обработка результатов сервиса.
        /// </summary>
        /// <param name="result">Результат.</param>
        /// <returns>Статус код.</returns>
        private IActionResult HandleServiceResult<T>(ServiceResult<T> result)
        {
            if (result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Data);
            }

            return Problem(detail: result.ErrorMessage, statusCode: result.StatusCode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private IActionResult HandleServiceResult(ServiceResult result)
        {
            if (result.IsSuccess)
            {
                return StatusCode(result.StatusCode);
            }
            return Problem(detail: result.ErrorMessage, statusCode: result.StatusCode);
        }


        /// <summary>
        /// 1. Создание нового пользователя.
        /// </summary>
        /// <remarks>
        /// Операция доступна только администраторам.
        /// Для выполнения действия укажите логин администратора в HTTP-заголовке 'X-Acting-User-Login'.
        /// </remarks>
        /// <param name="createUserDto">Данные для создания пользователя.</param>
        /// <response code="201">Пользователь успешно создан. Возвращает данные созданного пользователя.</response>
        /// <response code="400">Некорректный запрос (ошибки валидации).</response>
        /// <response code="401">Не указан заголовок X-Acting-User-Login.</response>
        /// <response code="403">Нет прав на выполнение операции (пользователь не администратор).</response>
        /// <response code="409">Пользователь с таким логином уже существует.</response>
        [HttpPost]
        [ProducesResponseType(typeof(UserViewDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            var actorLogin = GetActorLogin();

            if (string.IsNullOrEmpty(actorLogin))
            {
                return Problem(detail: "Заголовок X-Acting-User-Login обязателен.", statusCode: StatusCodes.Status401Unauthorized);
            }

            _logger.LogInformation("Attempt to create user by {ActorLogin}", actorLogin);

            return HandleServiceResult(await _userService.CreateUserAsync(createUserDto, actorLogin));
        }

        /// <summary>
        /// 2. Изменение имени, пола или даты рождения пользователя.
        /// </summary>
        /// <remarks>
        /// Операция доступна администратору для любого пользователя, либо самому пользователю для своих данных (если он активен).
        /// Для выполнения действия укажите логин выполняющего пользователя в HTTP-заголовке 'X-Acting-User-Login'.
        /// </remarks>
        /// <param name="login">Логин пользователя, чьи данные изменяются.</param>
        /// <param name="updateUserDto">Новые персональные данные пользователя.</param>
        /// <response code="200">Данные пользователя успешно обновлены. Возвращает обновленные данные пользователя.</response>
        /// <response code="400">Некорректный запрос (ошибки валидации).</response>
        /// <response code="401">Не указан заголовок X-Acting-User-Login или действующий пользователь не найден/не активен.</response>
        /// <response code="403">Нет прав на выполнение операции.</response>
        /// <response code="404">Пользователь для обновления не найден.</response>
        [HttpPut("{login}/personal-info")]
        [ProducesResponseType(typeof(UserViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserPersonalInfo(string login, [FromBody] UpdateUserPersonalInfoDto updateUserDto)
        {
            var actorLogin = GetActorLogin();

            if (string.IsNullOrEmpty(actorLogin))
            {
                return Problem(detail: "Заголовок X-Acting-User-Login обязателен.", statusCode: StatusCodes.Status401Unauthorized);
            }

            _logger.LogInformation("Attempt to update personal info for user {TargetUserLogin} by {ActorLogin}", login, actorLogin);

            return HandleServiceResult(await _userService.UpdateUserPersonalInfoAsync(login, updateUserDto, actorLogin));
        }

        /// <summary>
        /// 3. Изменение пароля пользователя.
        /// </summary>
        /// <remarks>
        /// Операция доступна администратору для любого пользователя, либо самому пользователю для своего пароля (если он активен).
        /// Для выполнения действия укажите логин выполняющего пользователя в HTTP-заголовке 'X-Acting-User-Login'.
        /// </remarks>
        /// <param name="login">Логин пользователя, чей пароль изменяется.</param>
        /// <param name="updatePasswordDto">Данные для обновления пароля.</param>
        /// <response code="200">Пароль пользователя успешно обновлен.</response>
        /// <response code="400">Некорректный запрос (ошибки валидации).</response>
        /// <response code="401">Не указан заголовок X-Acting-User-Login или действующий пользователь не найден/не активен.</response>
        /// <response code="403">Нет прав на выполнение операции.</response>
        /// <response code="404">Пользователь для обновления пароля не найден.</response>
        [HttpPut("{login}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserPassword(string login, [FromBody] UpdateUserPasswordDto updatePasswordDto)
        {
            var actorLogin = GetActorLogin();

            if (string.IsNullOrEmpty(actorLogin))
            {
                return Problem(detail: "Заголовок X-Acting-User-Login обязателен.", statusCode: StatusCodes.Status401Unauthorized);
            }

            _logger.LogInformation("Attempt to update password for user {TargetUserLogin} by {ActorLogin}", login, actorLogin);

            return HandleServiceResult(await _userService.UpdateUserPasswordAsync(login, updatePasswordDto, actorLogin));
        }

        /// <summary>
        /// 4. Изменение логина пользователя.
        /// </summary>
        /// <remarks>
        /// Операция доступна администратору для любого пользователя, либо самому пользователю для своего логина (если он активен).
        /// Новый логин должен быть уникальным.
        /// Для выполнения действия укажите логин выполняющего пользователя в HTTP-заголовке 'X-Acting-User-Login'.
        /// </remarks>
        /// <param name="login">Текущий логин пользователя, чей логин изменяется.</param>
        /// <param name="updateLoginDto">Данные для обновления логина (содержит новый логин).</param>
        /// <response code="200">Логин пользователя успешно обновлен. Возвращает обновленные данные пользователя.</response>
        /// <response code="400">Некорректный запрос (ошибки валидации).</response>
        /// <response code="401">Не указан заголовок X-Acting-User-Login или действующий пользователь не найден/не активен.</response>
        /// <response code="403">Нет прав на выполнение операции.</response>
        /// <response code="404">Пользователь для обновления логина не найден.</response>
        /// <response code="409">Новый логин уже занят другим пользователем.</response>
        [HttpPut("{login}/login")]
        [ProducesResponseType(typeof(UserViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateUserLogin(string login, [FromBody] UpdateUserLoginDto updateLoginDto)
        {
            var actorLogin = GetActorLogin();

            if (string.IsNullOrEmpty(actorLogin))
            {
                return Problem(detail: "Заголовок X-Acting-User-Login обязателен.", statusCode: StatusCodes.Status401Unauthorized);
            }

            _logger.LogInformation("Attempt to update login for user {TargetUserLogin} to {NewLogin} by {ActorLogin}", login, updateLoginDto.NewLogin, actorLogin);

            return HandleServiceResult(await _userService.UpdateUserLoginAsync(login, updateLoginDto, actorLogin));
        }

        /// <summary>
        /// 5. Запрос списка всех активных пользователей.
        /// </summary>
        /// <remarks>
        /// Операция доступна только администраторам. Список отсортирован по дате создания (CreatedOn).
        /// Для выполнения действия укажите логин администратора в HTTP-заголовке 'X-Acting-User-Login'.
        /// </remarks>
        /// <response code="200">Список активных пользователей успешно получен.</response>
        /// <response code="401">Не указан заголовок X-Acting-User-Login.</response>
        /// <response code="403">Нет прав на выполнение операции (пользователь не администратор).</response>
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<UserViewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetActiveUsers()
        {
            var actorLogin = GetActorLogin();

            if (string.IsNullOrEmpty(actorLogin))
            {
                return Problem(detail: "Заголовок X-Acting-User-Login обязателен.", statusCode: StatusCodes.Status401Unauthorized);
            }

            _logger.LogInformation("Attempt to get active users by {ActorLogin}", actorLogin);

            return HandleServiceResult(await _userService.GetActiveUsersAsync(actorLogin));
        }

        /// <summary>
        /// 6. Запрос пользователя по логину (краткая информация).
        /// </summary>
        /// <remarks>
        /// Операция доступна только администраторам. Возвращает имя, пол, дату рождения и статус активности пользователя.
        /// Для выполнения действия укажите логин администратора в HTTP-заголовке 'X-Acting-User-Login'.
        /// </remarks>
        /// <param name="login">Логин искомого пользователя.</param>
        /// <response code="200">Краткая информация о пользователе успешно получена.</response>
        /// <response code="401">Не указан заголовок X-Acting-User-Login.</response>
        /// <response code="403">Нет прав на выполнение операции (пользователь не администратор).</response>
        /// <response code="404">Пользователь с указанным логином не найден.</response>
        [HttpGet("{login}")]
        [ProducesResponseType(typeof(UserBriefViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserByLogin(string login)
        {
            var actorLogin = GetActorLogin();

            if (string.IsNullOrEmpty(actorLogin))
            {
                return Problem(detail: "Заголовок X-Acting-User-Login обязателен.", statusCode: StatusCodes.Status401Unauthorized);
            }

            _logger.LogInformation("Attempt to get user {TargetUserLogin} by {ActorLogin}", login, actorLogin);

            return HandleServiceResult(await _userService.GetUserByLoginAsync(login, actorLogin));
        }

        /// <summary>
        /// 7. Запрос полной информации о пользователе по его логину и паролю.
        /// </summary>
        /// <remarks>
        /// Операция доступна только самому пользователю, если он активен.
        /// Используется для "самопроверки" данных. Заголовок X-Acting-User-Login не требуется.
        /// </remarks>
        /// <param name="authRequest">Данные для аутентификации (логин и пароль).</param>
        /// <response code="200">Полная информация о пользователе успешно получена.</response>
        /// <response code="401">Неверный логин или пароль.</response>
        /// <response code="404">Пользователь не найден или не активен.</response>
        [HttpPost("authenticate")]
        [ProducesResponseType(typeof(UserViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AuthenticateSelf([FromBody] AuthenticateRequestDto authRequest)
        {
            _logger.LogInformation("Attempt to authenticate user {Login}", authRequest.Login);

            return HandleServiceResult(await _userService.GetUserForSelfAsync(authRequest.Login, authRequest.Password));
        }

        /// <summary>
        /// 8. Запрос всех пользователей старше определённого возраста.
        /// </summary>
        /// <remarks>
        /// Операция доступна только администраторам.
        /// Для выполнения действия укажите логин администратора в HTTP-заголовке 'X-Acting-User-Login'.
        /// </remarks>
        /// <param name="age">Минимальный возраст пользователей для выборки.</param>
        /// <response code="200">Список пользователей старше указанного возраста успешно получен.</response>
        /// <response code="400">Некорректное значение возраста.</response>
        /// <response code="401">Не указан заголовок X-Acting-User-Login.</response>
        /// <response code="403">Нет прав на выполнение операции (пользователь не администратор).</response>
        [HttpGet("older-than/{age}")]
        [ProducesResponseType(typeof(IEnumerable<UserViewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsersOlderThan(int age)
        {
            var actorLogin = GetActorLogin();

            if (string.IsNullOrEmpty(actorLogin))
            {
                return Problem(detail: "Заголовок X-Acting-User-Login обязателен.", statusCode: StatusCodes.Status401Unauthorized);
            }

            _logger.LogInformation("Attempt to get users older than {Age} by {ActorLogin}", age, actorLogin);

            return HandleServiceResult(await _userService.GetUsersOlderThanAsync(age, actorLogin));
        }

        /// <summary>
        /// 9. Удаление пользователя по логину (полное или "мягкое").
        /// </summary>
        /// <remarks>
        /// Операция доступна только администраторам. Администратор не может удалить сам себя.
        /// При "мягком" удалении устанавливаются поля RevokedOn и RevokedBy.
        /// При полном удалении пользователь удаляется из базы данных.
        /// Для выполнения действия укажите логин администратора в HTTP-заголовке 'X-Acting-User-Login'.
        /// </remarks>
        /// <param name="login">Логин пользователя для удаления.</param>
        /// <param name="hardDelete">Флаг: true для полного удаления, false (по умолчанию) для "мягкого" удаления.</param>
        /// <response code="200">Пользователь успешно "мягко" удален (или уже был "мягко" удален).</response>
        /// <response code="204">Пользователь успешно полностью удален.</response>
        /// <response code="401">Не указан заголовок X-Acting-User-Login.</response>
        /// <response code="403">Нет прав на выполнение операции (пользователь не администратор или попытка удалить самого себя).</response>
        /// <response code="404">Пользователь для удаления не найден.</response>
        [HttpDelete("{login}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string login, [FromQuery] bool hardDelete = false)
        {
            var actorLogin = GetActorLogin();

            if (string.IsNullOrEmpty(actorLogin))
            {
                return Problem(detail: "Заголовок X-Acting-User-Login обязателен.", statusCode: StatusCodes.Status401Unauthorized);
            }

            _logger.LogInformation("Attempt to {DeleteType} delete user {TargetUserLogin} by {ActorLogin}", hardDelete ? "hard" : "soft", login, actorLogin);

            return HandleServiceResult(await _userService.DeleteUserAsync(login, hardDelete, actorLogin));
        }

        /// <summary>
        /// 10. Восстановление "мягко" удаленного пользователя.
        /// </summary>
        /// <remarks>
        /// Операция доступна только администраторам. Очищает поля RevokedOn и RevokedBy.
        /// Для выполнения действия укажите логин администратора в HTTP-заголовке 'X-Acting-User-Login'.
        /// </remarks>
        /// <param name="login">Логин пользователя для восстановления.</param>
        /// <response code="200">Пользователь успешно восстановлен (или уже был активен). Возвращает данные пользователя.</response>
        /// <response code="401">Не указан заголовок X-Acting-User-Login.</response>
        /// <response code="403">Нет прав на выполнение операции (пользователь не администратор).</response>
        /// <response code="404">Пользователь для восстановления не найден.</response>
        [HttpPost("{login}/restore")]
        [ProducesResponseType(typeof(UserViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreUser(string login)
        {
            var actorLogin = GetActorLogin();

            if (string.IsNullOrEmpty(actorLogin))
            {
                return Problem(detail: "Заголовок X-Acting-User-Login обязателен.", statusCode: StatusCodes.Status401Unauthorized);
            }

            _logger.LogInformation("Attempt to restore user {TargetUserLogin} by {ActorLogin}", login, actorLogin);

            return HandleServiceResult(await _userService.RestoreUserAsync(login, actorLogin));
        }
    }
}