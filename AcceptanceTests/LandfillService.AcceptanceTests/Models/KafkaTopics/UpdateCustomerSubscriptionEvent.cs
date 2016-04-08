﻿using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace LandfillService.AcceptanceTests.Models.KafkaTopics
{
    public class UpdateCustomerSubscriptionEvent : ISubscriptionEvent
    {
        public Guid SubscriptionUID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
