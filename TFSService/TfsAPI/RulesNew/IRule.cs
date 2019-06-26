﻿using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsAPI.RulesNew
{
    public interface IRule
    {
        string Title {  get;}
        string Source { get;}

        string Condition { get;}

        [JsonConverter(typeof(StringEnumConverter))]
        RuleOperation Operation { get;}
    }

    public class Rule : IRule
    {
        public string Title { get; set;}
        public string Source { get; set; }
        public string Condition { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public RuleOperation Operation { get; set;}

        public override bool Equals(object obj)
        {
            return obj is Rule rule &&
                   Title == rule.Title &&
                   Source == rule.Source &&
                   Condition == rule.Condition &&
                   Operation == rule.Operation;
        }

        public override int GetHashCode()
        {
            var hashCode = -1018138163;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
            hashCode = hashCode * -1521134295 + Operation.GetHashCode();
            return hashCode;
        }
    }
}