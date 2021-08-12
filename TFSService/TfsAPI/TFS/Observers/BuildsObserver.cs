using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Build.WebApi.Events;
using Microsoft.VisualStudio.Services.Common;
using TfsAPI.Comparers;
using TfsAPI.Interfaces;

namespace TfsAPI.TFS.Observers
{
    public class BuildsObserver : ObserverCollectionBase<Build>, IBuildsObserver
    {
        private readonly IBuild _buildService;

        /// <inheritdoc/>
        public BuildsObserver(IBuild buildService, Action afterTick, IEnumerable<Build> saved, TimeSpan delay)
            : base(new IdBuildComparer(), new PartialBuildComparer(), afterTick, saved, delay)
        {
            _buildService = buildService;
        }

        public override Task<IList<Build>> Request()
        {
            return _buildService.GetRunningBuilds();
        }

        protected override async void OnCollectionChanged(CollectionChangeAction action, IList<Build> collection)
        {
            if (action == CollectionChangeAction.Remove)
            {
                // Нужно обновить коллекцию
                collection = await _buildService.Update(collection);
            }

            collection.Select(GetCorrectEvent).ForEach(x => BuildChanged?.Invoke(this, x));
        }

        public event EventHandler<BuildUpdatedEvent> BuildChanged;

        private BuildUpdatedEvent GetCorrectEvent(Build build)
        {
            switch (build.Status)
            {
                case BuildStatus.Completed:
                    return new BuildCompletedEvent(build);

                case BuildStatus.InProgress:
                case BuildStatus.NotStarted:
                case BuildStatus.Postponed:
                    return new BuildQueuedEvent(build);

                default:
                    return new BuildUpdatedEvent(build);
            }
        }
    }
}