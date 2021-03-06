﻿using Serilog.Core;
using Serilog.Events;

namespace VSS.Serilog.Extensions.Enrichers
{
  /// <summary>
  /// Custom Serilog enricher to parse out line feeds from exception stack messages.
  /// This is so the console output plays nice with FluentD/Kabana and we get only one 'entry' for the exception
  /// instead of one per line.
  /// </summary>
  /// <remarks>
  /// Replaces the default {Exception} enricher.
  /// </remarks>
  public class ExceptionEnricher : ILogEventEnricher
  {
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
      if (logEvent.Exception == null) { return; }

      var logEventProperty = propertyFactory.CreateProperty("EscapedException", logEvent.Exception.ToString().Replace("\r\n", "\\r\\n"));
      logEvent.AddPropertyIfAbsent(logEventProperty);
    }
  }
}
