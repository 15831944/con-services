﻿using System;

namespace LandfillService.AcceptanceTests.Models.KafkaTopics
{
    public class DissociateProjectSubscriptionEvent
    {
        public Guid SubscriptionUID { get; set; }

        public Guid ProjectUID { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }
    }
}
