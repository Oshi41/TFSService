using System;
using Microsoft.TeamFoundation.Work.WebApi;

namespace TfsAPI.Extentions
{
    public static class IterationExtentions
    {
        /// <summary>
        /// является ли это данной итерацией
        /// </summary>
        /// <param name="iteration"></param>
        /// <returns></returns>
        public static bool IsCurrent(this TeamSettingsIteration iteration)
        {
            if (iteration?.Attributes?.StartDate == null
                || iteration.Attributes?.FinishDate == null)
            {
                return false;
            }

            var now = DateTime.Now;

            return iteration.Attributes.StartDate <= now
                   && now <= iteration.Attributes.FinishDate;
        }
    }
}