﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Filter.Abstractions.Models.ResultHandling
{
  /// <summary>
  /// Single/List of filters returned from endpoint
  /// </summary>
  public class FilterDescriptorListResult : ContractExecutionResult, IMasterDataModel
  {
    /// <summary>
    /// Gets or sets the filter descriptors
    /// </summary>
    public ImmutableList<FilterDescriptor> FilterDescriptors { get; set; }

    public List<string> GetIdentifiers() => FilterDescriptors?
                                              .SelectMany(f => f.GetIdentifiers())
                                              .Distinct()
                                              .ToList()
                                            ?? new List<string>();
  }
}
