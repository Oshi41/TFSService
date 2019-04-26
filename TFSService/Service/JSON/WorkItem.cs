using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Newtonsoft.Json;

namespace Service.JSON
{
    public class WorkItem
    {
        [JsonConstructor]
        public WorkItem(string state, string itemType, string assignedTo, DateTime created)
        {
            State = state;
            ItemType = itemType;
            AssignedTo = assignedTo;
            Created = created;
        }

        public WorkItem(Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem source)
        {
            Created = DateTime.Parse(source[CoreField.CreatedDate].ToString());
            AssignedTo = source[CoreField.AssignedTo].ToString();
            ItemType = source[CoreField.WorkItemType].ToString();
        }

        #region Properties

        public DateTime Created { get; }

        public string AssignedTo { get; }

        public string ItemType { get; }

        public string State { get; }

        #endregion
    }
}
