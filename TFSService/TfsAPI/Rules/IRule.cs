using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsAPI.Rules
{
    public interface IRule
    {
        bool Passed();

        List<WorkItem> GetInconsistent();
    }
}
