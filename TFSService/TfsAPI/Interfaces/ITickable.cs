using System;
using System.Threading.Tasks;

namespace TfsAPI.Interfaces
{
    public interface ITickable
    {
        Task Tick();
        
        TimeSpan Delay { get; }
    }
}