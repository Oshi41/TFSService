using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.WebApi;

namespace TfsAPI.Comparers
{
    public class IdBuildComparer : IEqualityComparer<Build>
    {
        public bool Equals(Build x, Build y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(Build obj)
        {
            return obj.Id;
        }
    }
}