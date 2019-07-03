using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.Work.WebApi;
using TfsAPI.TFS.Capacity;

namespace TfsAPI.Extentions
{
    public static class IterationExtentions
    {
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

            return InRange(iteration.Attributes.StartDate.Value, iteration.Attributes.FinishDate.Value, start, end);
        }

        /// <summary>
        /// Ищет итерации, входящие в указанный промежуток времени
        /// </summary>
        /// <param name="iteration">Итерация</param>
        /// <param name="start">С какой даты ищем</param>
        /// <param name="end">Какой датой заканчиваем/param>
        /// <returns></returns>
        public static bool InRange(this NodeInfo iteration, DateTime start, DateTime end)
        {
            if (iteration?.StartDate == null || iteration?.FinishDate == null)
                return false;

            return InRange(iteration.StartDate.Value, iteration.FinishDate.Value, start, end);
        }

        public static bool InRange(this Iteration iteration, DateTime start, DateTime end)
        {
            if (iteration == null)
                return false;

            return InRange(iteration.Start, iteration.Finish, start, end);
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
        /// <summary>
        ///     является ли это данной итерацией
        /// </summary>
        /// <param name="iteration"></param>
        /// <returns></returns>
        public static bool IsCurrent(this TeamSettingsIteration iteration)
        {
            return IsCurrent(iteration?.Attributes?.StartDate, iteration?.Attributes?.FinishDate);
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

        private static bool InRange(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        {
            // Проверяем переменные
            if (end1 < start1)
            {
                Swap(ref start1, ref end1);
            }

            if (end2 < start2)
            {
                Swap(ref start2, ref end2);
            }


            // Начало или конец из одного промежутка обязательно должны попасть во второй временной отрезок

            // Первый отрезок
            //  Начало 1 орезка           Конец первого отрезка 
            // ----------S1-----------E1-------
            //           |            |
            // Пподходящие случаи     |
            // -S2-------|-----E2-----|-------- Попал в промежуток (начало вышло за пределы
            // ----------|---S2---E2--|-------- Полностью попал в промежуток 
            // ----------|-----S2-----|-----E2- Попал в промежуток (конец вышел за пределы

            var intersect = (start1 <= start2 && start2 <= end1)
                            ||
                            (start1 <= end2 && end2 <= end1);

            if (intersect)
                return true;

            // то же самое справедливо и наоборот

            return (start2 <= start1 && start1 <= end2)
                   ||    
                   (start2 <= end1 && end1 <= end2);
        }

        private static void Swap(ref DateTime x, ref DateTime y)
        {
            var temp = x;
            x = y;
            y = temp;
        }
    }
}