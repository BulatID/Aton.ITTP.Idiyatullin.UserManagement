using Aton.ITTP.Idiyatullin.UserManagement.Api.DTOs.User;

namespace Aton.ITTP.Idiyatullin.UserManagement.Api.Services
{
    /// <summary>
    /// Представляет результат выполнения операции сервиса, который может содержать данные.
    /// </summary>
    /// <typeparam name="T">Тип данных, возвращаемых в случае успеха.</typeparam>
    public class ServiceResult<T>
    {
        /// <summary>
        /// Данные результата операции. Null, если операция не удалась или не предполагает возврата данных.
        /// </summary>
        public T? Data { get; }

        /// <summary>
        /// Указывает, успешно ли завершилась операция.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Сообщение об ошибке, если операция не удалась. Null в случае успеха.
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// HTTP-статус код, соответствующий результату операции.
        /// </summary>
        public int StatusCode { get; }

        // Конструктор приватный, используйте фабричные методы Success и Failure.
        private ServiceResult(T? data, bool isSuccess, string? errorMessage, int statusCode)
        {
            Data = data;
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Создает успешный результат операции.
        /// </summary>
        /// <param name="data">Данные результата.</param>
        /// <param name="statusCode">HTTP-статус код (по умолчанию 200 OK).</param>
        /// <returns>Экземпляр ServiceResult с успешным результатом.</returns>
        public static ServiceResult<T> Success(T data, int statusCode = 200) => new ServiceResult<T>(data, true, null, statusCode);

        /// <summary>
        /// Создает результат операции, завершившейся ошибкой.
        /// </summary>
        /// <param name="errorMessage">Сообщение об ошибке.</param>
        /// <param name="statusCode">HTTP-статус код (по умолчанию 400 Bad Request).</param>
        /// <returns>Экземпляр ServiceResult с информацией об ошибке.</returns>
        public static ServiceResult<T> Failure(string errorMessage, int statusCode = 400) => new ServiceResult<T>(default, false, errorMessage, statusCode);
    }

    /// <summary>
    /// Представляет результат выполнения операции сервиса, которая не возвращает специфических данных.
    /// </summary>
    public class ServiceResult
    {
        /// <summary>
        /// Указывает, успешно ли завершилась операция.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Сообщение об ошибке, если операция не удалась. Null в случае успеха.
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// HTTP-статус код, соответствующий результату операции.
        /// </summary>
        public int StatusCode { get; }

        // Конструктор приватный, используйте фабричные методы Success и Failure.
        private ServiceResult(bool isSuccess, string? errorMessage, int statusCode)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Создает успешный результат операции.
        /// </summary>
        /// <param name="statusCode">HTTP-статус код (по умолчанию 204 No Content).</param>
        /// <returns>Экземпляр ServiceResult с успешным результатом.</returns>
        public static ServiceResult Success(int statusCode = 204) => new ServiceResult(true, null, statusCode);

        /// <summary>
        /// Создает результат операции, завершившейся ошибкой.
        /// </summary>
        /// <param name="errorMessage">Сообщение об ошибке.</param>
        /// <param name="statusCode">HTTP-статус код (по умолчанию 400 Bad Request).</param>
        /// <returns>Экземпляр ServiceResult с информацией об ошибке.</returns>
        public static ServiceResult Failure(string errorMessage, int statusCode = 400) => new ServiceResult(false, errorMessage, statusCode);
    }

    /// <summary>
    /// Интерфейс сервиса для управления пользователями.
    /// Определяет контракты для всех операций CRUD и других действий с пользователями.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Создает нового пользователя в системе.
        /// </summary>
        /// <param name="createUserDto">Данные для создания пользователя.</param>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратора).</param>
        /// <returns>Результат операции, содержащий DTO созданного пользователя или ошибку.</returns>
        Task<ServiceResult<UserViewDto>> CreateUserAsync(CreateUserDto createUserDto, string actorLogin);

        /// <summary>
        /// Обновляет персональную информацию указанного пользователя (имя, пол, дата рождения).
        /// </summary>
        /// <param name="targetUserLogin">Логин пользователя, чьи данные обновляются.</param>
        /// <param name="updateUserDto">Новые данные пользователя.</param>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратор или сам пользователь).</param>
        /// <returns>Результат операции, содержащий обновленный DTO пользователя или ошибку.</returns>
        Task<ServiceResult<UserViewDto>> UpdateUserPersonalInfoAsync(string targetUserLogin, UpdateUserPersonalInfoDto updateUserDto, string actorLogin);

        /// <summary>
        /// Обновляет пароль указанного пользователя.
        /// </summary>
        /// <param name="targetUserLogin">Логин пользователя, чей пароль обновляется.</param>
        /// <param name="updatePasswordDto">Данные для обновления пароля.</param>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратор или сам пользователь).</param>
        /// <returns>Результат операции (успех/ошибка).</returns>
        Task<ServiceResult> UpdateUserPasswordAsync(string targetUserLogin, UpdateUserPasswordDto updatePasswordDto, string actorLogin);

        /// <summary>
        /// Обновляет логин указанного пользователя. Логин должен оставаться уникальным.
        /// </summary>
        /// <param name="targetUserLogin">Текущий логин пользователя, чей логин обновляется.</param>
        /// <param name="updateLoginDto">Данные для обновления логина.</param>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратор или сам пользователь).</param>
        /// <returns>Результат операции, содержащий обновленный DTO пользователя (с новым логином) или ошибку.</returns>
        Task<ServiceResult<UserViewDto>> UpdateUserLoginAsync(string targetUserLogin, UpdateUserLoginDto updateLoginDto, string actorLogin);

        /// <summary>
        /// Получает список всех активных (не заблокированных) пользователей, отсортированных по дате создания.
        /// </summary>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратора).</param>
        /// <returns>Результат операции, содержащий коллекцию DTO активных пользователей или ошибку.</returns>
        Task<ServiceResult<IEnumerable<UserViewDto>>> GetActiveUsersAsync(string actorLogin);

        /// <summary>
        /// Получает краткую информацию (имя, пол, дата рождения, статус) о пользователе по его логину.
        /// </summary>
        /// <param name="targetUserLogin">Логин искомого пользователя.</param>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратора).</param>
        /// <returns>Результат операции, содержащий DTO с краткой информацией о пользователе или ошибку.</returns>
        Task<ServiceResult<UserBriefViewDto>> GetUserByLoginAsync(string targetUserLogin, string actorLogin);

        /// <summary>
        /// Получает полную информацию о пользователе для самого пользователя на основе логина и пароля.
        /// Используется для самопроверки данных пользователем.
        /// </summary>
        /// <param name="targetUserLogin">Логин пользователя.</param>
        /// <param name="password">Пароль пользователя.</param>
        /// <returns>Результат операции, содержащий DTO пользователя или ошибку (например, при неверном пароле).</returns>
        Task<ServiceResult<UserViewDto>> GetUserForSelfAsync(string targetUserLogin, string password);

        /// <summary>
        /// Получает список всех пользователей, которые старше указанного возраста.
        /// </summary>
        /// <param name="age">Возраст, старше которого должны быть пользователи.</param>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратора).</param>
        /// <returns>Результат операции, содержащий коллекцию DTO пользователей старше заданного возраста или ошибку.</returns>
        Task<ServiceResult<IEnumerable<UserViewDto>>> GetUsersOlderThanAsync(int age, string actorLogin);

        /// <summary>
        /// Удаляет пользователя. Удаление может быть "мягким" (установка флагов RevokedOn, RevokedBy) или полным.
        /// </summary>
        /// <param name="targetUserLogin">Логин пользователя для удаления.</param>
        /// <param name="hardDelete">Флаг, указывающий на тип удаления: true - полное удаление, false - "мягкое" удаление.</param>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратора).</param>
        /// <returns>Результат операции (успех/ошибка).</returns>
        Task<ServiceResult> DeleteUserAsync(string targetUserLogin, bool hardDelete, string actorLogin);

        /// <summary>
        /// Восстанавливает "мягко" удаленного пользователя (очищает поля RevokedOn, RevokedBy).
        /// </summary>
        /// <param name="targetUserLogin">Логин пользователя для восстановления.</param>
        /// <param name="actorLogin">Логин пользователя, выполняющего операцию (администратора).</param>
        /// <returns>Результат операции, содержащий DTO восстановленного пользователя или ошибку.</returns>
        Task<ServiceResult<UserViewDto>> RestoreUserAsync(string targetUserLogin, string actorLogin);
    }
}