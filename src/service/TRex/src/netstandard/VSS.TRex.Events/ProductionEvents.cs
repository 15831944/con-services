﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities;
using VSS.TRex.IO.Helpers;
using Range = VSS.TRex.Common.Utilities.Range;

namespace VSS.TRex.Events
{
  public class ProductionEvents
  {
    /// <summary>
    /// The machine to which these events relate
    /// </summary>
    public short MachineID { get; set; }

    /// <summary>
    /// The event type this list stores
    /// </summary>
    public ProductionEventType EventListType { get; protected set; } = ProductionEventType.Unknown;

    public string EventChangeListPersistantFileName() => EventChangeListPersistantFileName(MachineID, EventListType);

    public static string EventChangeListPersistantFileName(short machineID, ProductionEventType eventListType) => $"{machineID}-Events-{eventListType}-Summary.evt";
  }

  /// <summary>
  /// ProductionEvents implements a generic event list without using class instances for each event
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ProductionEvents<T> : ProductionEvents, IProductionEvents<T>
  {
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ProductionEvents<T>>();

    private byte VERSION_NUMBER = 1;

    private const int MinStreamLength = 9;

    public bool EventsChanged { get; set; }

    private readonly Func<T, T, bool> _eventStateComparator;

    /// <summary>
    /// The structure that contains all information about this type of event.
    /// All events occur at a point in time, have some optional flags and contain a state 
    /// defined by the generic T type
    /// </summary>
    public struct Event
    {
      /// <summary>
      /// Flag constant indicating this event is a customer event
      /// </summary>
      private const int kCustomEventBitFlag = 0;

      /// <summary>
      /// Defines whether this event is a custom event, ie: an event that was not recorded by a machine but which has been 
      /// inserted as a part of another process such as to override values recorded by the machine that were incorrect 
      /// (eg: design or material lift number)
      /// </summary>
      public bool IsCustomEvent
      {
        get => BitFlagHelper.IsBitOn(Flags, kCustomEventBitFlag);
        set => BitFlagHelper.SetBit(ref Flags, kCustomEventBitFlag, value);
      }

      /// <summary>
      /// The date/time at which this event occurred.
      /// </summary>
      public DateTime Date;

      /// <summary>
      /// State defines the value of the generic event type. whose type is defined by the T generic type.
      /// It is assigned the default value for the type. Make sure all enumerated and other types specify an
      /// appropriate default (or null) value
      /// </summary>
      public T State;

      public byte Flags;
    }

    /// <summary>
    /// The Site Model to which these events relate
    /// </summary>
    public Guid SiteModelID { get; set; }

//        private DateTime lastUpdateTimeUTC = DateTime.MinValue;

    /// <summary>
    /// Records the time at which this event change list was last updated in the persistent store
    /// </summary>
//        public DateTime LastUpdateTimeUTC { get => lastUpdateTimeUTC; set => lastUpdateTimeUTC = value; }

    public Action<BinaryWriter, T> SerialiseStateOut { get; set; }

    public Func<BinaryReader, T> SerialiseStateIn { get; set; }

    // private bool eventsListIsOutOfDate;

    // The list containing all the time ordered instances of this event type
    public List<Event> Events = new List<Event>();

    public ProductionEvents(short machineID, Guid siteModelID,
      ProductionEventType eventListType,
      Action<BinaryWriter, T> serialiseStateOut,
      Func<BinaryReader, T> serialiseStateIn,
      Func<T, T, bool> eventStateComparator)
    {
      MachineID = machineID;
      SiteModelID = siteModelID;
      EventListType = eventListType;

      // Machines created with the max machine ID are treated as transient and never
      // stored in or loaded from the FS file. 
      // LoadedFromPersistentStore = machineID == kICMachineIDMaxValue;

      SerialiseStateIn = serialiseStateIn;
      SerialiseStateOut = serialiseStateOut;

      _eventStateComparator = eventStateComparator;
    }

    public bool EventsEquivalent(Event event1, Event event2) => !event1.IsCustomEvent && !event2.IsCustomEvent && _eventStateComparator(event1.State, event2.State);

    // Compare performs a date based comparison between the event identified
    // by <Item> and the date held in <Value>
    public bool Find(DateTime findDate, out int index)
    {
      if (findDate.Kind != DateTimeKind.Utc)
        throw new ArgumentException("FindDate must be a UTC date time", nameof(findDate));

      int L = 0;
      int H = Events.Count - 1;

      while (L <= H)
      {
        int I = (L + H) >> 1;
        int C = Events[I].Date.CompareTo(findDate);

        if (C < 0)
        {
          L = I + 1;
        }
        else
        {
          H = I - 1;
          if (C == 0)
          {
            index = I;
            return true;
          }
        }
      }

      index = L;
      return false;
    }

    public virtual bool Find(Event value, out int index)
    {
      return Find(value.Date, out index);
    }

    //    function UpgradeEventListFile(const FileStream : TStream;
    //                                  const InternalStream: TMemoryStream;
    //                                  const FileMajorVersion, FileMinorVersion: Integer): Boolean; virtual;

    // public bool EventsListIsOutOfDate() => eventsListIsOutOfDate;

    // protected bool LoadedFromPersistentStore = false;

    /// <summary>
    /// Retrieves the event state and date at a specific location in the events list
    /// </summary>
    /// <param name="index"></param>
    /// <param name="dateTime"></param>
    /// <param name="state"></param>
    public void GetStateAtIndex(int index, out DateTime dateTime, out T state)
    {
      dateTime = Events[index].Date;
      state = Events[index].State;
    }

    /// <summary>
    /// Modifies the event state at a specific location in the events list
    /// </summary>
    /// <param name="index"></param>
    /// <param name="state"></param>
    public void SetStateAtIndex(int index, T state)
    {
      Events[index] = new Event
      {
        Date = Events[index].Date,
        State = state
      };
    }

    /// <summary>
    /// Adds an event of type T with the given date into the list. 
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="value"></param>
    /// <returns>The event instance that was added to the list</returns>
    public virtual void PutValueAtDate(DateTime dateTime, T value)
    {
      if (dateTime.Kind != DateTimeKind.Utc)
        throw new TRexException("dateTime must be a UTC date time");

      PutValueAtDate(new Event
      {
        Date = dateTime,
        State = value,
      });
    }

    /// <summary>
    /// Adds a set of event of type T with the given dates and values into the list. 
    /// </summary>
    /// <param name="events"></param>
    /// <returns>The event instance that was added to the list</returns>
    public virtual void PutValuesAtDates(IEnumerable<(DateTime, T)> events)
    {
      foreach (var evt in events)
      {
        PutValueAtDate(new Event
        {
          Date = evt.Item1,
          State = evt.Item2,
        });
      }
    }

    /// <summary>
    /// Removes the event with the given datetime from the list if it exists.
    /// </summary>
    public void RemoveValueAtDate(DateTime dateTime)
    {
      if (Find(dateTime, out int index))
      {
        Events.RemoveAt(index);
        EventsChanged = true;
      }
    }

    /// <summary>
    /// Clears all events from the list
    /// </summary>
    public void Clear()
    {
      if (Events.Count > 0)
      {
        Events.Clear();
        EventsChanged = true;
      }
    }

    /// <summary>
    /// Creates an array of string representations for the events in the list bounded by the
    /// supplied date range and the maximum number of events to return
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="maxEventsToReturn"></param>
    /// <returns></returns>
    public List<string> ToStrings(DateTime startDate, DateTime endDate, int maxEventsToReturn)
    {
      if (startDate.Kind != DateTimeKind.Utc || startDate.Kind != DateTimeKind.Utc)
        throw new TRexException("Start and end dates must be a UTC date times");

      // Get the index of the first value
      GetValueAtDate(startDate, out int index);

      // If the start date is before the first event index will be -1, compensate for that
      index = index < 0 ? 0 : index;

      var result = new List<string>(maxEventsToReturn);

      for (int i = index; i < Math.Min(index + maxEventsToReturn, Events.Count); i++)
      {
        if (Events[i].Date > endDate)
          break;

        result.Add($"Date: {Events[i].Date}, Event:{Events[i].State}");
      }

      return result;
    }

    /// <summary>
    /// Adds the given event into the list.  If the event is a duplicate of an existing event the 
    /// passed event will be ignored and the existing duplicate event will be returned, otherwise 
    /// passed event will be returned.
    /// The method returns the event instance that was added to the list
    /// </summary>
    /// <param name="Event"></param>
    /// <returns>The event instance that was added to the list</returns>
    public virtual void PutValueAtDate(Event Event)
    {
      bool ExistingEventFound = Find(Event.Date, out int EventIndex);

      if (ExistingEventFound)
      {
        if (Events[EventIndex].Date != Event.Date)
          throw new TRexException($"Two events are the same but that they have different dates in {nameof(PutValueAtDate)}");

        // If we find an event with the same date then delete the existing one and replace it with the new one.
        bool CorrectInsertLocationIdentified;
        do
        {
          CorrectInsertLocationIdentified = true;

          if (ExistingEventFound)
          {
            // If we've got a machine event overriding a machine event or a custom event overriding a custom event
            // then delete the existing event.
            if (Events[EventIndex].IsCustomEvent == Event.IsCustomEvent)
            {
              if (Event.IsCustomEvent)
              {
                Log.LogDebug($"Deleting custom machine event: {Events[EventIndex]}");
                Events.RemoveAt(EventIndex);
              }
              else
              {
                // All start/end recorded data events to have duplicates as these are
                // needed for correction collation
                if (EventListType != ProductionEventType.StartEndRecordedData)
                {
                  // 'Delete' the duplicate by not adding it
                  return;
                }

                throw new TRexException("Start/End recorded events should not be managed by the generic event PutValueAtDate()");
              }
            }
            else if (Event.IsCustomEvent)
            {
              // If we've got a custom event with the same date as a machine event
              // then "bump" the custom event's date by a (millisecond) to ensure it's
              // after the machine event.

              Event.Date = Event.Date.AddMilliseconds(1);
              CorrectInsertLocationIdentified = false;
            }
          }

          ExistingEventFound = Find(Event, out EventIndex);
        } while (!CorrectInsertLocationIdentified);
      }

      Events.Insert(EventIndex, Event);

      EventsChanged = true;
    }

    /// <summary>
    /// Collates a series of events within an event list by aggregating consecutive events where there is no change
    /// in the underlying event state
    /// </summary>
    public virtual void Collate(IProductionEventLists container)
    {
      bool HaveStartEndEventPair = false;

      DateTime StartEvent = Consts.MIN_DATETIME_AS_UTC; 
      DateTime EndEvent = Consts.MAX_DATETIME_AS_UTC;

      int FirstIdx = 0;
      int SecondIdx = 1;

      // We only want to collate items generally if they fall between a pair of Start/EndRecordedData events.
      // The EventStartEndRecordedDataChangeList.Collate method overrides this one to collate those
      // Start/EndRecordedData events slightly differently.
      // All other Container.EventStartEndRecordedData should use this method.
      // This method also relies on the fact that the Container.EventStartEndRecordedData instance should
      // have been correctly collated BEFORE any of the other Container event lists are
      // collated; this is currently achieved by the fact that ProductionEventChanges.SaveToFile saves
      // the EventStartEndRecordedData list first, indirectly invoking Collate on that list first, before
      // saving the rest of the event lists.
      while (SecondIdx < Events.Count)
      {
        if (!HaveStartEndEventPair ||
            !Range.InRange(Events[FirstIdx].Date, StartEvent, EndEvent))
        {
          if (!container.GetStartEndRecordedDataEvents().FindStartEventPairAtTime(Events[FirstIdx].Date, out StartEvent, out EndEvent))
          {
            FirstIdx = SecondIdx;
            SecondIdx = FirstIdx + 1;

            continue;
          }

          HaveStartEndEventPair = true;
        }

        if (EventsEquivalent(Events[FirstIdx], Events[SecondIdx]) &&
            Range.InRange(Events[FirstIdx].Date, StartEvent, EndEvent) &&
            Range.InRange(Events[SecondIdx].Date, StartEvent, EndEvent))
        {
          EventsChanged = true;
          Events.RemoveAt(SecondIdx);
        }
        else
        {
          FirstIdx = SecondIdx;
          SecondIdx++;
        }
      }
    }

    //    property EventChangeDataSize: Int64 read FEventChangeDataSize;
    //    procedure DumpToText(const FileName: TFileName;
    //                         const IncludeFileNameHeader : Boolean;
    //                         const NumberEvents : Boolean;
    //                         const IncludeFilenameInDump : Boolean);

    /// <summary>
    /// Determines the index of the event whose date immediately precedes the given eventData
    /// </summary>
    /// <param name="eventDate"></param>
    /// <returns></returns>
    public int IndexOfClosestEventPriorToDate(DateTime eventDate)
    {
      if (Events.Count == 0 || (Events.Count > 0 && Events[0].Date > eventDate))
        return -1;

      bool FindResult = Find(eventDate, out int LastIndex);

      // We're looking for the event prior to the requested date.
      // If we didn't find an exact match for requested date, then
      // LastIndex will be the event subsequent to the requested date,
      // so subtract one from LastIndex to give us the event prior
      if (!FindResult && LastIndex > 0)
        LastIndex--;

      return LastIndex;
    }

    /// <summary>
    /// Determines the index of the event whose date immediately follows the given eventData
    /// </summary>
    /// <param name="eventDate"></param>
    /// <returns></returns>
    public int IndexOfClosestEventSubsequentToDate(DateTime eventDate)
    {
      if (Events.Count == 0 || (Events.Count > 0 && Events[Events.Count - 1].Date < eventDate))
        return -1;

      Find(eventDate, out int LastIndex);

      return LastIndex;
    }

    // Merges Start/End events into an event list to enable easy navigation for things like the timeline
    // procedure AddStartEndEvents(StartStopEvents: TICProductionEventChangeList);

    // Function CalculateInMemorySize : Integer; Virtual;
    // Function InMemorySize : Integer; InLine;
    // Procedure MarkEventListAsInMemoryOnly; Inline;

    public MemoryStream GetMutableStream()
    {
      var mutableStream = RecyclableMemoryStreamManagerHelper.Manager.GetStream();
      using (var writer = new BinaryWriter(mutableStream, Encoding.Default, true))
      {
        VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

        writer.Write((int) EventListType);
        writer.Write(Events.Count);
        foreach (var e in Events)
        {
          writer.Write(e.Date.ToBinary());
          writer.Write(e.Flags);
          SerialiseStateOut(writer, e.State);
        }
      }

      return mutableStream;
    }

    /// <summary>
    /// remove successive, duplicate events. Events have been sorted by date (def says these are 'time ordered')
    /// </summary>
    /// <returns></returns>
    public MemoryStream GetImmutableStream()
    {
      var immutableStream = RecyclableMemoryStreamManagerHelper.Manager.GetStream();

      using (var immutableWriter = new BinaryWriter(immutableStream, Encoding.UTF8, true))
      {
        VersionSerializationHelper.EmitVersionByte(immutableWriter, VERSION_NUMBER);

        immutableWriter.Write((int) EventListType);

        T lastState = Events[0].State;
        var filteredEventCount = 0;
        var countPosition = immutableWriter.BaseStream.Position;
        immutableWriter.Write(filteredEventCount);
        for (int i = 0; i < Events.Count; i++)
        {
          if (i == 0 || (i > 0 && !_eventStateComparator(Events[i].State, lastState)))
          {
            immutableWriter.Write(Events[i].Date.ToBinary());
            immutableWriter.Write(Events[i].Flags);
            SerialiseStateOut(immutableWriter, Events[i].State);
            filteredEventCount++;
          }

          lastState = Events[i].State;
        }

        var eosPosition = immutableWriter.BaseStream.Position;
        immutableWriter.BaseStream.Position = countPosition;
        immutableWriter.Write(filteredEventCount);
        immutableWriter.BaseStream.Position = eosPosition;

        // Log.LogDebug($"Converted event list to immutable for event {typeof(T).Name}. Mutable count = {Events.Count}, immutable count = {filteredEventCount}");
      }

      return immutableStream;
    }

    /// <summary>
    /// Serializes the events and stores the serialized represented in the persistent store
    /// </summary>
    /// <param name="storageProxy"></param>
    public void SaveToStore(IStorageProxy storageProxy)
    {
      using (var mutableStream = GetMutableStream())
      {
        storageProxy.WriteStreamToPersistentStore(SiteModelID, EventChangeListPersistantFileName(), FileSystemStreamType.Events, mutableStream, this);
      }

      EventsChanged = false;
    }

    public void RemoveFromStore(IStorageProxy storageProxy)
    {
      var result = storageProxy.RemoveStreamFromPersistentStore(SiteModelID, FileSystemStreamType.Events, EventChangeListPersistantFileName());

      if (result != FileSystemErrorStatus.OK)
      {
        Log.LogInformation($"Error {result} occurred removing {EventChangeListPersistantFileName()} from project {SiteModelID}");
        return;
      }

      EventsChanged = false;
    }

    /// <summary>
    /// Loads the event list by requesting its serialized representation from the persistent store and 
    /// deserializing it into the event list
    /// </summary>
    /// <param name="storageProxy"></param>
    /// <returns></returns>
    public void LoadFromStore(IStorageProxy storageProxy)
    {
      storageProxy.ReadStreamFromPersistentStore(SiteModelID, EventChangeListPersistantFileName(),
        FileSystemStreamType.Events, out MemoryStream MS);

      if (MS != null)
      {
        using (MS)
        {
          MS.Position = 0;
          using (var reader = new BinaryReader(MS))
          {
            ReadEvents(reader);
          }
        }
      }
    }

    public void ReadEvents(BinaryReader reader)
    {
      if (reader.BaseStream.Length < MinStreamLength)
      {
        throw new TRexException($"ProductionEvent mutable stream length is too short. Expected greater than: {MinStreamLength} retrieved {reader.BaseStream.Length}.");
      }

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      var eventType = reader.ReadInt32();
      if (!Enum.IsDefined(typeof(ProductionEventType), eventType))
      {
        throw new TRexException("ProductionEvent eventType is not recognized. Invalid stream.");
      }

      int count = reader.ReadInt32();
      Events.Clear();
      Events.Capacity = count;

      for (int i = 0; i < count; i++)
      {
        Events.Add(new Event
        {
          Date = DateTime.FromBinary(reader.ReadInt64()),
          Flags = reader.ReadByte(),
          State = SerialiseStateIn(reader)
        });
      }
    }

    /// <summary>
    /// Returns a generic object reference to the internal list of events in this list
    /// The purpose of this is to facilitate CopyEventsFrom
    /// </summary>
    /// <returns></returns>
    public object RawEventsObjects() => Events;

    /// <summary>
    /// Locates and returns the event occurring at or immediately prior to the given eventDate
    /// </summary>
    /// <param name="eventDate"></param>
    /// <param name="stateChangeIndex"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public virtual T GetValueAtDate(DateTime eventDate, out int stateChangeIndex, T defaultValue = default)
    {
      T result = defaultValue;

      if (Events.Count == 0)
      {
        stateChangeIndex = -1;
      }
      else
      {
        if (!Find(eventDate, out stateChangeIndex))
          stateChangeIndex--;

        if (stateChangeIndex >= 0)
        {
          Event StateChange = Events[stateChangeIndex];

          if (StateChange.Date <= eventDate)
            result = StateChange.State;
        }
      }

      return result;
    }

    /// <summary>
    /// Basic event sorting comparator based solely on Date
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    protected virtual int Compare(Event a, Event b) => a.Date.CompareTo(b.Date);

    /// <summary>
    /// Sorts the events in the list according to the semantics of plain event lists, or start/end event lists.
    /// </summary>
    public void Sort() => Events.Sort(Compare);

    /// <summary>
    /// Returns the count of elements present in the Event list
    /// </summary>
    /// <returns></returns>
    public int Count() => Events.Count;

    /// <summary>
    /// Provides the last element in the event list. Will through IndexOutOfRange exception if list is empty.
    /// </summary>
    /// <returns></returns>
    public Event Last() => Events[Events.Count - 1];

    /// <summary>
    /// Copies all events from the source list to this list.
    /// Note: The end result of this operation will not be a sorted list at the new events are added
    /// to the end of the list and require collation to sort & remove duplicates
    /// </summary>
    /// <param name="eventsList"></param>
    public void CopyEventsFrom(IProductionEvents eventsList)
    {
      // If the numbers of events being added here become significant, then
      // it may be worth using an event merge process similar to the one done
      // in cell pass integration

      foreach (Event Event in (List<Event>) eventsList.RawEventsObjects())
        PutValueAtDate(Event);
    }

    /// <summary>
    /// Copies all events from the source list to this list.
    /// Note: The end result of this operation will not be a sorted list at the new events are added
    /// to the end of the list and require collation to sort & remove duplicates
    /// </summary>
    /// <param name="eventsList"></param>
    public void CopyEventsFrom(IProductionEvents<T> eventsList)
    {
      // If the numbers of events being added here become significant, then
      // it may be worth using an event merge process similar to the one done
      // in cell pass integration

      foreach (Event Event in ((ProductionEvents<T>) eventsList).Events)
        PutValueAtDate(Event);
    }

    /// <summary>
    /// Returns the state of the last element in the events list
    /// </summary>
    /// <returns></returns>
    public T LastStateValue(T defaultValue = default(T)) => Events.Count == 0 ? defaultValue : Events.Last().State;

    /// <summary>
    /// Returns the date of the last element in the events list
    /// </summary>
    /// <returns></returns>
    public DateTime LastStateDate() => Events.Last().Date;

    /// <summary>
    /// Returns the date of the first element in the events list
    /// </summary>
    /// <returns></returns>
    public DateTime FirstStateDate() => Events.First().Date;
  }
}
