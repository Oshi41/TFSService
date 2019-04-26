using System;

namespace TfsAPI.Extentions
{
    public static class DateTimeExtentions
    {
        /// <summary>
        /// Дата сегодняшняя?
        /// </summary>
        /// <param name="time">Время, которое надо проверить</param>
        /// <param name="today">Для оптимизации можно передать <see cref="DateTime.Now"/></param>
        /// <returns></returns>
        public static bool IsToday(this DateTime time, DateTime? today = null)
        {
            return time.Date == (today?.Date ?? DateTime.Today.Date);
        }
    }
}
