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

        /// <summary>
        /// Даты из одного месяца
        /// </summary>
        public static bool SameMonth(this DateTime x, DateTime y)
        {
            return x.Year == y.Year && x.Month == y.Month;
        }

        /// <summary>
        /// Даты приблизительно равны, в пределах нескольких минут
        /// </summary>
        /// <param name="source">С каким временем сверяем</param>
        /// <param name="time">Что сравниваем</param>
        /// <param name="minutes">Кол-во минут в радиусе которых алгоритм вернет совпадение</param>
        /// <returns></returns>
        public static bool IsNear(this DateTime source, DateTime time, uint minutes = 10)
        {
            var from = time.AddMinutes(-minutes);
            var to = time.AddMinutes(minutes);

            return from <= time && time <= to;
        }
    }
}
