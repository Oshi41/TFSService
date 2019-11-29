using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.Extentions
{
    public static class RevisionExtensions
    {
        public static bool? ChangedBy(this Revision revision, string name)
        {
            if (revision?.Fields == null || name == null)
                return null;

            var assignedTo = revision.Fields.TryGetById((int) CoreField.ChangedBy)?.Value;
            if (assignedTo == null)
                return null;

            return Equals(assignedTo, name);
        }

        /// <summary>
        /// Когда имзенение произошло
        /// </summary>
        /// <param name="revision"></param>
        /// <returns></returns>
        public static DateTime ChangedDate(this Revision revision)
        {
            return revision?.Fields?.TryGetById((int) CoreField.ChangedDate)?.Value as DateTime? ?? DateTime.MinValue;
        }

        /// <summary>
        /// Сколько осталось работать
        /// </summary>
        /// <param name="revision"></param>
        /// <returns></returns>
        public static double RemainingWork(this Revision revision)
        {
            return revision?.Fields["Remaining Work"]?.Value as double? ?? -1.0;
        }
    }
}