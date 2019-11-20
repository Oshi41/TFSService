using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.Extentions
{
    public static class WorkItemCollectionExtensions
    {
        public static Dictionary<T1, T2> ToDictionary<T1, T2>(this WorkItemCollection collection,
            Func<WorkItem, T1> keyFunc,
            Func<WorkItem, T2> valFunc,
            bool noThrow)
        {
            var dict = new Dictionary<T1, T2>();

            for (int i = 0; i < collection.Count; i++)
            {
                var item = collection[i];

                var key = keyFunc(item);
                var value = valFunc(item);

                // Пропускаем выброс исключения
                if (dict.ContainsKey(key) && noThrow)
                    continue;

                dict.Add(key, value);
            }

            return dict;
        }

        public static Dictionary<T1, WorkItem> ToDictionary<T1>(this WorkItemCollection collection,
            Func<WorkItem, T1> keyFunc)
        {
            return collection.ToDictionary(keyFunc, item => item, false);
        }

        public static IList<WorkItem> Except(this WorkItemCollection source, WorkItemCollection except,
            IEqualityComparer<WorkItem> comparer)
        {
            return source.ToList().Except(except.ToList(), comparer);
        }

        public static IList<WorkItem> Except(this IEnumerable<WorkItem> source, WorkItemCollection except, IEqualityComparer<WorkItem> comparer)
        {
            return source
                .Except(except.ToList(),
                    comparer);
        }

        public static IList<WorkItem> Except(this WorkItemCollection source, IEnumerable<WorkItem> except, IEqualityComparer<WorkItem> comparer)
        {
            return source.ToList()
                .Except(except,
                    comparer);
        }

        public static IList<WorkItem> Except(this IEnumerable<WorkItem> source, IEnumerable<WorkItem> except,
            IEqualityComparer<WorkItem> comparer)
        {
            var calculatedSource = source.ToList();
            var calculatedExcept = except.ToList();

            var result = new List<WorkItem>();

            foreach (var left in calculatedSource)
            {
                // Вычислили хэш
                var hash = comparer.GetHashCode(left);
                // Ищем такой же в списке на исключение, сначала по хэшу, потом сравниваем
                var find = calculatedExcept.FirstOrDefault(x => comparer.GetHashCode(x).Equals(hash) && comparer.Equals(x, left));

                // Если не нашли в списке на исключение, добавляем в результат
                if (find == null)
                {
                    result.Add(left);
                }
            }

            return result;
        }

        public static IList<T> Select<T>(this WorkItemCollection source, Func<WorkItem, T> func)
        {
            var list = new List<T>();

            for (int i = 0; i < source.Count; i++)
            {
                list.Add(func(source[i]));
            }

            return list;
        }

        public static IList<WorkItem> ToList(this WorkItemCollection source)
        {
            return source.Select(x => x);
        }

        public static IList<WorkItem> Where(this WorkItemCollection source, Predicate<WorkItem> func)
        {
            var list = new List<WorkItem>();

            for (int i = 0; i < source.Count; i++)
            {
                var item = source[i];
                if (func(item))
                {
                    list.Add(item);
                }
            }

            return list;
        }
    }
}
