using System;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.Work.WebApi;

namespace TfsAPI.Extentions
{
    public static class IterationExtentions
    {
        /// <summary>
        ///     является ли это данной итерацией
        /// </summary>
        /// <param name="iteration"></param>
        /// <returns></returns>
        public static bool IsCurrent(this TeamSettingsIteration iteration)
        {
            return IsCurrent(iteration?.Attributes?.StartDate, iteration?.Attributes?.FinishDate);
        }

        /// <summary>
        /// Ищет итерации, входящие в указанный промежуток времени
        /// </summary>
        /// <param name="iteration">Итерация</param>
        /// <param name="start">С какой даты ищем</param>
        /// <param name="end">Какой датой заканчиваем/param>
        /// <returns></returns>
        public static bool InRange(this TeamSettingsIteration iteration, DateTime start, DateTime end)
        {
            if (iteration?.Attributes?.StartDate == null || iteration?.Attributes?.FinishDate == null)
                return false;

            var iterationStart = iteration.Attributes.StartDate.Value;
            var iterationEnd = iteration.Attributes.FinishDate.Value;

            // Заданный отрезок времени пересекает промежуток итерации

            //   Нач. итерации            Конец итерации
            // ------B---------------------E----
            //       |                     |
            // Подходящие случаи           |
            // --B---|----------E----------|----        Находится в итерации (начало не попало)
            // ------|----------B----------|---E--      Находится в итерации (конец не попал)
            // ------|------B----E---------|----        Полностью находится в итерации
            var intersect = (start <= iterationStart && iterationStart <= end)
                            ||
                            (start <= iterationEnd && iterationEnd <= end);

            if (intersect)
                return true;

            // Итерация пересекает указанный промежуток времени

            //   Нач. промежутка            Конец промежутка
            // ------B---------------------E----
            //       |                     |
            // Подходящие случаи итерации  |
            // --B---|----------E----------|----        Находится в промежутке (начало не попало)
            // ------|----------B----------|---E--      Находится в промежутке (конец не попал)
            // ------|------B----E---------|----        Полностью находится в промежутке
            intersect = (iterationStart <= start && start <= iterationEnd)
                        ||
                        (iterationStart <= end && end <= iterationStart);

            return intersect;
        }

        /// <summary>
        /// Является ли это данной итерацией
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool IsCurrent(this NodeInfo info)
        {
            return IsCurrent(info?.StartDate, info?.FinishDate);
        }

        private static bool IsCurrent(DateTime? start, DateTime? finish)
        {
            if (start.HasValue && finish.HasValue)
            {
                var now = DateTime.Now.Date;

                return start.Value.Date <= now
                       && now <= finish.Value.Date;
            }

            return false;
        }
    }
}