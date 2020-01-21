using System.Collections.Generic;
using TfsAPI.Interfaces;

namespace Gui.ViewModels.Filter
{
    public interface IInitializable
    {
        /// <summary>
        /// Устанавливает список рабочих элементов в подсказке
        /// </summary>
        /// <param name="all"></param>
        /// <param name="api"></param>
        void Initialize(IEnumerable<WorkItemVm> all, ITfsApi api = null);
    }
}