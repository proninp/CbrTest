using System;

namespace Sberkorus.Cbr.Extensions
{
    /// <summary>
    /// Расширения для работы с датой.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Преобразует дату в строку в формате "yyyy-MM-dd".
        /// </summary>
        /// <param name="dateTime">Дата для преобразования.</param>
        /// <returns>Строковое представление даты.</returns>
        public static string ToDateString(this DateTime dateTime) =>
            dateTime.ToString("yyyy-MM-dd");
    }
}