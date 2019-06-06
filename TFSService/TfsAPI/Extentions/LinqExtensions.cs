using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsAPI.Extentions
{
    public static class LinqExtensions
    {
        public static bool IsTermwiseEquals(this IEnumerable source, IEnumerable sequence, IEqualityComparer<object> comparer = null)
        {
            if (source == null && sequence == null)
                return true;

            if (source == null || sequence == null)
                return false;

            var x = source.OfType<object>().ToList();
            var y = sequence.OfType<object>().ToList();

            if (x.Count() != y.Count())
                return false;

            var except = x.Except(y, comparer ?? EqualityComparer<object>.Default);
            
            return !except.Any();
        }
    }
}
