namespace Aton.ITTP.Idiyatullin.UserManagement.Api.Helpers
{
    /// <summary>
	/// Валидаторы.
	/// </summary>
	public struct ValidationHelper
    {
        /// <summary>
        /// Проверяет параметр на null.
        /// </summary>
        /// <param name="value">Объект.</param>
        /// <exception cref="ArgumentNullException">Объект равен null.</exception>
        public static void IsNull(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Значение параметра равен null.");
            }
        }

        /// <summary>
		/// Проверка строки на пустоту.
		/// </summary>
		/// <param name="text">Текст.</param>
		/// <exception cref="ArgumentException">Если текст пустой.</exception>
		public static void IsStringNullOrEmpty(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Строка пустая.");
            }
        }
    }
}