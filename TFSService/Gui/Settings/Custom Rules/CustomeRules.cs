using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using TfsAPI.Interfaces;
using TfsAPI.TFS;

namespace Gui.Settings.Custom_Rules
{
    public class CustomeRules
    {
        public string Querry { get; set; }

        public List<WorkItem> GetInconsistentItems(TfsApi api)
        {

        }
    }
}
