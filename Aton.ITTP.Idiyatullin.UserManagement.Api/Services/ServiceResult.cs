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

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ServiceResult{T}"/> с указанными параметрами.
        /// </summary>
        /// <remarks>
        /// Этот конструктор является приватным. Для создания экземпляров следует использовать
        /// статические фабричные методы <see cref="Success(T, int)"/> и <see cref="Failure(string, int)"/>.
        /// </remarks>
        /// <param name="data">Данные результата операции. Может быть null.</param>
        /// <param name="isSuccess">Значение, указывающее, успешно ли завершилась операция.</param>
        /// <param name="errorMessage">Сообщение об ошибке, если операция не удалась. Может быть null.</param>
        /// <param name="statusCode">HTTP-статус код, соответствующий результату операции.</param>
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

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="isSuccess">Успешно ли.</param>
        /// <param name="errorMessage">Текст ошибкм.</param>
        /// <param name="statusCode">Статус-код.</param>
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
}