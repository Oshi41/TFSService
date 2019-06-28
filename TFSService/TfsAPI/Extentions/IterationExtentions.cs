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