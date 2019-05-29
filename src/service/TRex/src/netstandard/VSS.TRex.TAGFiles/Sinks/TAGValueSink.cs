﻿using System;
using System.Collections.Generic;
using System.Linq;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Classes.ValueMatcher;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.TAGFiles.Classes.Sinks
{
    /// <summary>
    /// Implements a sink against all defined TAG values.
    /// </summary>
    public class TAGValueSink : TAGValueSinkBase
    {
        /// <summary>
        /// Processor responsible for accepting TAG values matched by the TAG value matcher
        /// </summary>
        protected TAGProcessorStateBase Processor { get; }

        /// <summary>
        /// The set of value matchers available to match TAG values being accepted
        /// </summary>
        private static readonly Dictionary<string, TAGValueMatcher> ValueMatchers = InitialiseValueMatchers();

        /// <summary>
        /// Returns the list of TAGs that are supported by this instance of the TAG value sink
        /// </summary>
        public static readonly string[] InstantiatedTAGs = ValueMatchers.Keys.ToArray();

        /// <summary>
        /// Local value matcher state that the TAG value matchers use to coordinate values before sending them to the processor
        /// </summary>
        protected TAGValueMatcherState ValueMatcherState { get; } = new TAGValueMatcherState();

        /// <summary>
        /// Locate all value matcher classes and add them to the value matchers list using reflection (or just manually as below)
        /// </summary>
        private static Dictionary<string, TAGValueMatcher> InitialiseValueMatchers()
        {
            // Get all the value matcher classes that exist in the assembly. These are all classes that
            // descend from TAGValueMatcher
            var matchers = TypesHelper.FindAllDerivedTypes<TAGValueMatcher>();

            var valueMatchers = new Dictionary<string, TAGValueMatcher>();

            // Iterate through those types and create each on in turn, query the TAG types from it that the matcher supports and
            // then register the value matcher instance against those TAGs to allow the TAG file processor to locate matcher for TAGS
            foreach (var t in matchers)
            {
                var matcher = (TAGValueMatcher)Activator.CreateInstance(t);

                foreach (string tag in matcher.MatchedValueTypes())
                {
                    valueMatchers.Add(tag, matcher);
                }
            }

            return valueMatchers;
        }

        public TAGValueSink(TAGProcessorStateBase processor)
        {
            Processor = processor;
        }

        public override bool Starting() => true;

        public override bool Finishing()
        {
            //Check if we need to process a final context
            return !ValueMatcherState.HaveSeenATimeValue || Processor.ProcessEpochContext();
        }

        public override void ReadANSIStringValue(TAGDictionaryItem valueType, byte[] value)
        {
            if (ValueMatchers.TryGetValue(valueType.Name, out TAGValueMatcher valueMatcher))
            {
                valueMatcher?.ProcessANSIStringValue(ValueMatcherState, Processor, valueType, value);
            }
        }

        public override void ReadEmptyValue(TAGDictionaryItem valueType)
        {
            if (ValueMatchers.TryGetValue(valueType.Name, out TAGValueMatcher valueMatcher))
            {
                valueMatcher?.ProcessEmptyValue(ValueMatcherState, Processor, valueType);
            }
        }

        public override void ReadIEEEDoubleValue(TAGDictionaryItem valueType, double value)
        {
            if (ValueMatchers.TryGetValue(valueType.Name, out TAGValueMatcher valueMatcher))
            {
                valueMatcher?.ProcessDoubleValue(ValueMatcherState, Processor, valueType, value);
            }
        }

        public override void ReadIEEESingleValue(TAGDictionaryItem valueType, float value)
        {
           // Don't care - apparently no Single TAG names have ever been defined
        }

        public override void ReadIntegerValue(TAGDictionaryItem valueType, int value)
        {
            if (ValueMatchers.TryGetValue(valueType.Name, out TAGValueMatcher valueMatcher))
            {
                valueMatcher?.ProcessIntegerValue(ValueMatcherState, Processor, valueType, value);
            }
        }

        public override void ReadUnicodeStringValue(TAGDictionaryItem valueType, string value)
        {
            if (ValueMatchers.TryGetValue(valueType.Name, out TAGValueMatcher valueMatcher))
            {
                valueMatcher?.ProcessUnicodeStringValue(ValueMatcherState, Processor, valueType, value);
            }
        }

        public override void ReadUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (ValueMatchers.TryGetValue(valueType.Name, out TAGValueMatcher valueMatcher))
            {
                valueMatcher?.ProcessUnsignedIntegerValue(ValueMatcherState, Processor, valueType, value);
            }
        }
    }
}
