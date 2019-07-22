using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TfsAPI.Extentions
{
    public static class LinqExtensions
    {
        /// <summary>
        ///     Сравнивает 2 списка поэлементно, порядок не важен
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sequence"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static bool IsTermwiseEquals(this IEnumerable source, IEnumerable sequence,
            IEqualityComparer<object> comparer = null)
        {
            return IsTermwiseEquals(source.OfType<object>(), sequence.OfType<object>(), comparer);
        }

        public static bool IsTermwiseEquals<T>(this IEnumerable<T> source, IEnumerable<T> sequence,
            IEqualityComparer<T> comparer = null)
        {
            if (source == null && sequence == null)
                return true;

            if (source == null || sequence == null)
                return false;

            var x = source.ToList();
            var y = sequence.ToList();

            if (x.Count != y.Count)
                return false;

            var except = x.Except(y, comparer ?? EqualityComparer<T>.Default);

            return !except.Any();
        }
    }
}