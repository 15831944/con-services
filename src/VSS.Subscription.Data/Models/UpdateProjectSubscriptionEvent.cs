﻿using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using VSS.Subscription.Model.Interfaces;

namespace VSS.Subscription.Data.Models
{
    public class UpdateProjectSubscriptionEvent : ISubscriptionEvent
    {
        [Required]
        public Guid SubscriptionUID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid? CustomerUID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SubscriptionType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? StartDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? EndDate { get; set; }

        [Required]
        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }
    }
}
