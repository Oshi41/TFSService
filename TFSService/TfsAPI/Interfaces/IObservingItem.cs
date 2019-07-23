using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.Interfaces
{
    public interface IObservingItem
    {
        int Id { get; }
        DateTime LastChange { get; }
    }
}
