using System;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Build.WebApi.Events;

namespace TfsAPI.Interfaces
{
    public interface IBuildsObserver : ICollectionObserver<Build>
    {
        /// <summary>
        /// Состояние сборки изменилось.
        /// Может быть 3 типа ивента:
        /// <see cref="BuildCompletedEvent"/>
        /// <para>Если статус сборки выставлен в <see cref="BuildStatus.Completed"/></para>
        /// <see cref="BuildQueuedEvent"/>
        /// <para>Если статус сборки выставлен в <see cref="BuildStatus.InProgress"/> либо
        /// <see cref="BuildStatus.NotStarted"/> либо <see cref="BuildStatus.Postponed"/></para>
        /// <see cref="BuildUpdatedEvent"/>
        /// <para>Во всех других случаях</para>
        /// </summary>
        event EventHandler<BuildUpdatedEvent> BuildChanged;
    }
}