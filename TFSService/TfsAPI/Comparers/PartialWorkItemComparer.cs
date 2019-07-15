using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.Comparers
{
    class PartialWorkItemComparer : IEqualityComparer<WorkItem>
    {
        public bool Equals(WorkItem x, WorkItem y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return true;

            // Поле последнего изменения считаем за основу.
            // Изменилось оно, значит что-то произошло
            return x.Id == y.Id
                   && SafeEqualsCoreFields(x.Fields, y.Fields, CoreField.ChangedDate);
        }

        private bool SafeEqualsCoreFields(FieldCollection x, FieldCollection y, params CoreField[] fields)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            foreach (var field in fields)
            {
                // Судя по исходному коду, ID можно получить прмым кастом из CoreField

                var xField = x.TryGetById((int) field);
                var yField = y.TryGetById((int) field);

                // У всех рабочих элементов нет такого поля, пропускаем
                if (xField == null && yField == null)
                {
                    continue;
                }

                // У одного из элементов нет поля
                if (xField == null || yField == null)
                    return false;

                // Проверяем значения
                if (!Equals(xField.Value, yField.Value))
                    return false;
            }

            return true;
        }

        public int GetHashCode(WorkItem obj)
        {
            return obj.Id;
        }
    }
}
