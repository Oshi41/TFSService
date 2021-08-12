using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.WebApi;

namespace TfsAPI.Comparers
{
    public class PartialBuildComparer : IEqualityComparer<Build>
    {
        public bool Equals(Build x, Build y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Id == y.Id && x.Status == y.Status && x.Result == y.Result &&
                   Nullable.Equals(x.FinishTime, y.FinishTime) && x.LastChangedDate.Equals(y.LastChangedDate);
        }

        public int GetHashCode(Build obj)
        {
            unchecked
            {
                var hashCode = obj.Id;
                hashCode = (hashCode * 397) ^ obj.Status.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Result.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.FinishTime.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.LastChangedDate.GetHashCode();
                return hashCode;
            }
        }
    }
}