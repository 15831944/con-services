﻿using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Filter.Abstractions.Models.ResultHandling
{
  /// <summary>
  /// Single filter descriptor
  /// </summary>
  public class FilterDescriptorSingleResult : ContractExecutionResult
  {
    public FilterDescriptorSingleResult(FilterDescriptor filterDescriptor)
    {
      FilterDescriptor = filterDescriptor;
    }

    /// <summary>
    /// Gets or sets the filter descriptor.
    /// </summary>
    public FilterDescriptor FilterDescriptor { get; set; }
  }
}
