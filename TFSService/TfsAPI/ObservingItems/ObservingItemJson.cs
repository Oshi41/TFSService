using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Newtonsoft.Json;
using TfsAPI.Interfaces;

namespace TfsAPI.ObservingItems
{
    public class ObservingItemJson : IObservingItem
    {
        [JsonConstructor]
        public ObservingItemJson(DateTime lastChange, int id)
        {
            LastChange = lastChange;
            Id = id;
        }

        public ObservingItemJson(WorkItem item)
            : this(GetLastChangedDate(item), item?.Id ?? -1)
        {
        }

        public int Id { get; }
        public DateTime LastChange { get; }

        protected bool Equals(ObservingItemJson other)
        {
            return Id == other.Id 
                   && LastChange.Equals(other.LastChange);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) 
                return false;
            if (ReferenceEquals(this, obj)) 
                return true;

            if (obj.GetType() != GetType()) 
                return false;

            return Equals((ObservingItemJson)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id * 397) ^ LastChange.GetHashCode();
            }
        }

        private static DateTime GetLastChangedDate(WorkItem item)
        {
            return item?.Fields[CoreField.ChangedDate]?.Value is DateTime time
                ? time
                : DateTime.MinValue;
        }
    }

}
