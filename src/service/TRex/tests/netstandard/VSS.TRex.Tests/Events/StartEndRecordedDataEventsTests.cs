﻿using System;
using System.Collections.Generic;
using System.Linq;
using VSS.TRex.Events;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Events
{
    public class StartEndRecordedDataEventsTests : IClassFixture<DILoggingFixture>
  {
        [Fact]
        public void Test_StartEndRecordedDataEvents_Creation()
        {
            StartEndProductionEvents events = new StartEndProductionEvents(-1, Guid.Empty,
                ProductionEventType.StartEndRecordedData,
                (w, s) => w.Write((byte)s),
                r => (ProductionEventType)r.ReadByte());

            Assert.True(null != events, "Failed to create events list");
            Assert.True(ProductionEventType.StartEndRecordedData == events.EventListType, "Incorrect event list type");
            Assert.True(0 == events.Events.Count, "New list is not empty");
            Assert.True(-1 == events.MachineID, "Machine ID not -1");
            Assert.True(Guid.Empty == events.SiteModelID, "Site model ID is not null");
            Assert.True(null != events.SerialiseStateIn, "SerialiseStateIn is null");
            Assert.True(null != events.SerialiseStateOut, "SerialiseStateOut is null");
        }

        [Fact]
        public void Test_StartEndRecordedDataEvents_SimpleStartEndCollation()
        {
            StartEndProductionEvents events =
                new StartEndProductionEvents(-1, Guid.Empty, ProductionEventType.StartEndRecordedData, null, null);

            var firstEventDate = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 0, 0, 0), DateTimeKind.Utc);
            var secondEventDate = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 0, 0), DateTimeKind.Utc);

            void CheckEvents()
            {
                Assert.True(firstEventDate == events.Events[0].Date, $"Date of first element incorrect, expected {firstEventDate}, got {events.Events[0].Date}");
                Assert.True(ProductionEventType.StartEvent == events.Events[0].State, $"State of first element incorrect, expected {ProductionEventType.StartEvent}, got {events.Events[0].State}");

                Assert.True(secondEventDate == events.Events[1].Date, $"Date of second element incorrect, expected {secondEventDate}, got {events.Events[1].Date}");
                Assert.True(ProductionEventType.EndEvent == events.Events[1].State, $"State of second element incorrect, expected {ProductionEventType.EndEvent}, got {events.Events[1].State}");
            }

            // Add a single start and end at different dates and ensure they are both present and ordered correctly, before and after collation
            events.PutValueAtDate(firstEventDate, ProductionEventType.StartEvent);
            events.PutValueAtDate(secondEventDate, ProductionEventType.EndEvent);

            Assert.True(2 == events.Count(), $"List contains {events.Count()} events, instead of 2");

            // Check the state is good
            CheckEvents();

            // Collate the events and ensure nothing changes
            events.Collate(null);

            // Check the state is still good
            CheckEvents();
        }

        public static DateTime outerFirstEventDate = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 0, 0, 0), DateTimeKind.Utc);
        public static DateTime innerFirstEventDate = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 0, 10, 0), DateTimeKind.Utc);
        public static DateTime innerSecondEventDate = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 0, 20, 0), DateTimeKind.Utc);
        public static DateTime outerSecondEventDate = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 0, 0), DateTimeKind.Utc);

        public static IEnumerable<object[]> InnerOuterEventDates(int numTests)
        {
            var allData = new List<object[]>
            {
                new object[] {outerFirstEventDate, innerFirstEventDate, innerSecondEventDate, outerSecondEventDate},
                new object[] {outerFirstEventDate, outerFirstEventDate, outerSecondEventDate, outerSecondEventDate}
            };

            return allData.Take(numTests);
        }

        [Theory]
        [MemberData(nameof(InnerOuterEventDates), parameters: 4)]
        public void Test_StartEndRecordedDataEvents_DuplicatedAndNestedStartEndAndCollation
            (DateTime outerFirstEventDate, DateTime innerFirstEventDate,
            DateTime innerSecondEventDate, DateTime outerSecondEventDate)
        {
            StartEndProductionEvents events =
                new StartEndProductionEvents(-1, Guid.Empty, ProductionEventType.StartEndRecordedData, null, null);

            void CheckEventsBefore(int count)
            {
                if (count >= 1)
                {
                    Assert.True(outerFirstEventDate == events.Events[0].Date,
                        $"Count {count}: Date of outer first element incorrect, expected {outerFirstEventDate}, got {events.Events[0].Date}");
                    Assert.True(ProductionEventType.StartEvent == events.Events[0].State,
                        $"Count {count}: State of outer first element incorrect, expected {ProductionEventType.StartEvent}, got {events.Events[0].State}");
                }

                if (count >= 2)
                {
                    Assert.True(innerFirstEventDate == events.Events[1].Date,
                        $"Count {count}: Date of inner first element incorrect, expected {innerFirstEventDate}, got {events.Events[1].Date}");
                    Assert.True(ProductionEventType.StartEvent == events.Events[1].State,
                        $"Count {count}: State of inner first element incorrect, expected {ProductionEventType.StartEvent}, got {events.Events[1].State}");
                }

                if (count >= 3)
                {
                    Assert.True(innerSecondEventDate == events.Events[2].Date,
                        $"Count {count}: Date of inner second element incorrect, expected {innerSecondEventDate}, got {events.Events[2].Date}");
                    Assert.True(ProductionEventType.EndEvent == events.Events[2].State,
                        $"Count {count}: State of inner second element incorrect, expected {ProductionEventType.EndEvent}, got {events.Events[2].State}");
                }

                if (count >= 4)
                {
                    Assert.True(outerSecondEventDate == events.Events[3].Date,
                        $"Count {count}: Date of outer second element incorrect, expected {outerSecondEventDate}, got {events.Events[3].Date}");
                    Assert.True(ProductionEventType.EndEvent == events.Events[3].State,
                        $"Count {count}: State of outer second element incorrect, expected {ProductionEventType.EndEvent}, got {events.Events[3].State}");
                }
            }

            void CheckEventsAfter()
            {
                Assert.True(events.Count() == 2, "Events count not two after collation");
                Assert.True(outerFirstEventDate == events.Events[0].Date, $"Date of first outer element incorrect, expected {outerFirstEventDate}, got {events.Events[0].Date}");
                Assert.True(ProductionEventType.StartEvent == events.Events[0].State, $"State of first outer element incorrect, expected {ProductionEventType.StartEvent}, got {events.Events[0].State}");

                Assert.True(outerSecondEventDate == events.Events[1].Date, $"Date of second outer element incorrect, expected {outerSecondEventDate}, got {events.Events[1].Date}");
                Assert.True(ProductionEventType.EndEvent == events.Events[1].State, $"State of second outer element incorrect, expected {ProductionEventType.EndEvent}, got {events.Events[1].State}");
            }

            // Add nested start and end events at different dates and ensure they are both present and ordered correctly, before and after collation
            events.PutValueAtDate(outerFirstEventDate, ProductionEventType.StartEvent);
            CheckEventsBefore(1);
            events.PutValueAtDate(innerFirstEventDate, ProductionEventType.StartEvent);
            CheckEventsBefore(2);
            events.PutValueAtDate(innerSecondEventDate, ProductionEventType.EndEvent);
            CheckEventsBefore(3);
            events.PutValueAtDate(outerSecondEventDate, ProductionEventType.EndEvent);
            CheckEventsBefore(4);

            Assert.True(4 == events.Count(), $"List contains {events.Count()} events, instead of 4");

            // Check the four elements are as expected
            CheckEventsBefore(4);

            // Collate the events and ensure nothing changes
            events.Collate(null);

            // Check the resulting 2 elements are as expected
            CheckEventsAfter();
        }

      [Fact]
      void Test_StartEndRecordedDataEvents_ComplexNestedStartEndCollation()
      {
        StartEndProductionEvents events = new StartEndProductionEvents(-1, Guid.Empty, ProductionEventType.StartEndRecordedData, null, null);
      
        // Construct an array of 50 dates one minute apart
        var dateTimes = Enumerable.Range(0, 50).Select(x => DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, x, 0), DateTimeKind.Utc)).ToArray();

        // Make simplest event list
        events.PutValueAtDate(dateTimes[0], ProductionEventType.StartEvent);
        events.PutValueAtDate(dateTimes[dateTimes.Length - 1], ProductionEventType.EndEvent);

        events.Collate(null);
        Assert.True(events.Count() == 2, $"Event count not 2 after initial collation (length is {events.Count()})");
        Assert.True(events.Events[0].State == ProductionEventType.StartEvent, "First event not start event");
        Assert.True(events.Events[1].State == ProductionEventType.EndEvent, "Last event not end event");

        // mimic many additions of start end events completely covering the wider interval
        for (int i = 0; i < dateTimes.Length - 1; i++)
        {
          events.PutValueAtDate(dateTimes[i], ProductionEventType.StartEvent);
          events.PutValueAtDate(dateTimes[i + 1], ProductionEventType.EndEvent);
        }

        events.Collate(null);
        Assert.True(events.Count() == 2, $"Event count not 2 after collation of internal start/end pairs (length is {events.Count()})");
        Assert.True(events.Events[0].State == ProductionEventType.StartEvent, "First event not start event");
        Assert.True(events.Events[1].State == ProductionEventType.EndEvent, "Last event not end event");
    }


      [Fact]
      void Test_StartEndRecordedDataEvents_ComplexNonNestedStartEndCollation_Ordered()
      {
        StartEndProductionEvents events = new StartEndProductionEvents(-1, Guid.Empty, ProductionEventType.StartEndRecordedData, null, null);

        // Construct an array of 50 dates one minute apart
        var dateTimes = Enumerable.Range(0, 50).Select(x => DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, x, 0), DateTimeKind.Utc)).ToArray();

        // mimic many additions of start end events completely covering the wider interval
        for (int i = 0; i < dateTimes.Length - 1; i++)
        {
          events.PutValueAtDate(dateTimes[i], ProductionEventType.StartEvent);
          events.PutValueAtDate(dateTimes[i + 1], ProductionEventType.EndEvent);
        }

        events.Collate(null);
        Assert.True(events.Count() == 2, $"Event count not 2 after collation of internal start/end pairs (length is {events.Count()})");
        Assert.True(events.Events[0].State == ProductionEventType.StartEvent, "First event not start event");
        Assert.True(events.Events[1].State == ProductionEventType.EndEvent, "Last event not end event");
      }

      [Fact]
      void Test_StartEndRecordedDataEvents_ComplexNonNestedStartEndCollation_Unordered()
      {
        StartEndProductionEvents events = new StartEndProductionEvents(-1, Guid.Empty, ProductionEventType.StartEndRecordedData, null, null);

        // Construct an array of 50 dates one minute apart
        var dateTimes = Enumerable.Range(0, 50).Select(x => DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, x, 0), DateTimeKind.Utc)).ToArray();

        // mimic many additions of start end events completely covering the wider interval
        for (int i = dateTimes.Length - 2; i >= 0; i--)
        {
          events.PutValueAtDate(dateTimes[i], ProductionEventType.StartEvent);
          events.PutValueAtDate(dateTimes[i + 1], ProductionEventType.EndEvent);
        }

        events.Collate(null);
        Assert.True(events.Count() == 2, $"Event count not 2 after collation of internal start/end pairs (length is {events.Count()})");
        Assert.True(events.Events[0].State == ProductionEventType.StartEvent, "First event not start event");
        Assert.True(events.Events[1].State == ProductionEventType.EndEvent, "Last event not end event");
      }

      [Fact]
      void Test_StartEndRecordedDataEvents_OverlappedNeighbourStartStopSorting()
      {
        StartEndProductionEvents events = new StartEndProductionEvents(-1, Guid.Empty, ProductionEventType.StartEndRecordedData, null, null);

        // Add four events that should result in the following order -> Start-Start-End-End before sorting
        events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 0, 0), DateTimeKind.Utc), ProductionEventType.StartEvent);
        events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 0), DateTimeKind.Utc), ProductionEventType.EndEvent);
        events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 0), DateTimeKind.Utc), ProductionEventType.StartEvent);
        events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 2, 0), DateTimeKind.Utc), ProductionEventType.EndEvent);

        Assert.True(events.Events.Count == 4);
        Assert.True(events.Events[0].State == ProductionEventType.StartEvent, "Unexpected order before sorting");
        Assert.True(events.Events[1].State == ProductionEventType.StartEvent, "Unexpected order before sorting");
        Assert.True(events.Events[2].State == ProductionEventType.EndEvent, "Unexpected order before sorting");
        Assert.True(events.Events[3].State == ProductionEventType.EndEvent, "Unexpected order before sorting");

        // Sort them, after which the order should be Start-End-Start-End
        events.Sort();

        Assert.True(events.Events.Count == 4);
        Assert.True(events.Events[0].State == ProductionEventType.StartEvent, "Unexpected order after sorting");
        Assert.True(events.Events[1].State == ProductionEventType.EndEvent, "Unexpected order after sorting");
        Assert.True(events.Events[2].State == ProductionEventType.StartEvent, "Unexpected order after sorting");
        Assert.True(events.Events[3].State == ProductionEventType.EndEvent, "Unexpected order after sorting");

        events.Collate(null);
        Assert.True(events.Count() == 2, $"Event count not 2 after collation of internal start/end pairs (length is {events.Count()})");
        Assert.True(events.Events[0].State == ProductionEventType.StartEvent, "First event not start event");
        Assert.True(events.Events[1].State == ProductionEventType.EndEvent, "Last event not end event");
      }

    /// <summary>
    /// Due to jitter in how close the last TAG file time and following first TAG file time we allow a one second
    /// slop between those times to consider them equal
    /// </summary>
    [Fact]
    void Test_StartEndRecordedDataEvents_SloppyStartStopTimeSorting()
    {
      // Add four events that should result in two separate start end pairs
      StartEndProductionEvents events = new StartEndProductionEvents(-1, Guid.Empty, ProductionEventType.StartEndRecordedData, null, null);
      events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 0, 0, 0), DateTimeKind.Utc), ProductionEventType.StartEvent);
      events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1, 0), DateTimeKind.Utc), ProductionEventType.EndEvent);
      events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 3, 0), DateTimeKind.Utc), ProductionEventType.StartEvent);
      events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 2, 5, 0), DateTimeKind.Utc), ProductionEventType.EndEvent);

      events.Collate(null);
      Assert.True(events.Count() == 4, $"[No slop] Event count not 4 after collation of internal start/end pairs (length is {events.Count()})");

      // Add four events that should result in one separate start end pair, with just < 1 second slop
      events = new StartEndProductionEvents(-1, Guid.Empty, ProductionEventType.StartEndRecordedData, null, null);
      events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 0, 0, 0), DateTimeKind.Utc), ProductionEventType.StartEvent);
      events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1, 1), DateTimeKind.Utc), ProductionEventType.EndEvent);
      events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 2, 0), DateTimeKind.Utc), ProductionEventType.StartEvent);
      events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 2, 5, 0), DateTimeKind.Utc), ProductionEventType.EndEvent);

      events.Collate(null);
      Assert.True(events.Count() == 2, $"[Slop < 1000ms] Event count not 2 after collation of internal start/end pairs (length is {events.Count()})");

      // Add four events that should result in two separate start end pairs, with > 1 second slop
      events = new StartEndProductionEvents(-1, Guid.Empty, ProductionEventType.StartEndRecordedData, null, null);
      events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 0, 0, 0), DateTimeKind.Utc), ProductionEventType.StartEvent);
      events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1, 0), DateTimeKind.Utc), ProductionEventType.EndEvent);
      events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 2, 1), DateTimeKind.Utc), ProductionEventType.StartEvent);
      events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 2, 5, 0), DateTimeKind.Utc), ProductionEventType.EndEvent);

      events.Collate(null);
      Assert.True(events.Count() == 4, $"[Slop > 1000ms] Event count not 4 after collation of internal start/end pairs (length is {events.Count()})");
    }

    [Fact]
      void Test_StartEndRecordedDataEvents_EquivalentTo()
      {
        StartEndProductionEvents events = new StartEndProductionEvents(-1, Guid.Empty, ProductionEventType.StartEndRecordedData, null, null);

        events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 0, 0), DateTimeKind.Utc), ProductionEventType.StartEvent);
        events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 0), DateTimeKind.Utc), ProductionEventType.EndEvent);
        events.PutValueAtDate(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 2, 0), DateTimeKind.Utc), ProductionEventType.StartEvent);

        Assert.False(events.EventsEquivalent(events.Events[0], events.Events[1]), "Events 0 & 1 are equivalent-to when they are not");
        Assert.True(events.EventsEquivalent(events.Events[0], events.Events[2]), "Events 1 & 2 are not equivalent-to when they are");
      }

      [Fact]
      void Test_StartEndRecordedDataEvents_EqualityComparer()
      {
        Assert.True(EqualityComparer<ProductionEventType>.Default.Equals(ProductionEventType.StartEvent, ProductionEventType.StartEvent));
        Assert.True(EqualityComparer<ProductionEventType>.Default.Equals(ProductionEventType.EndEvent, ProductionEventType.EndEvent));
        Assert.False(EqualityComparer<ProductionEventType>.Default.Equals(ProductionEventType.StartEvent, ProductionEventType.EndEvent));
      }
  }
}
