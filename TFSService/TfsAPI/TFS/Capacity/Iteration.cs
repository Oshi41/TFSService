﻿using System;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.Work.WebApi;

namespace TfsAPI.TFS.Capacity
{
    /// <summary>
    ///     представление итерации TFS
    /// </summary>
    public class Iteration
    {
        public Iteration(Guid id, DateTime start, DateTime finish)
        {
            Id = id;
            Start = start;
            Finish = finish;
        }

        public Iteration(TeamSettingsIteration iter)
            : this(iter.Id, iter.Attributes.StartDate.Value, iter.Attributes.FinishDate.Value)
        {
        }

        public Iteration(NodeInfo info)
        {
            var raw = info.Uri;
            var pos = raw.LastIndexOf("/");

            if (pos > 0)
            {
                raw = raw.Substring(++pos);
                if (Guid.TryParse(raw, out var guid))
                {
                    Id = guid;
                    Start = info.StartDate.Value;
                    Finish = info.FinishDate.Value;
                }
            }
        }

        public Guid Id { get; }

        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
    }
}