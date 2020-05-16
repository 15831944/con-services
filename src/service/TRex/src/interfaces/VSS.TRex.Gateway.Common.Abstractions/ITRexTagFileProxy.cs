﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.TRex.Gateway.Common.Abstractions
{
  public interface ITRexTagFileProxy
  {
    Task<ContractExecutionResult> SendTagFileDirect(CompactionTagFileRequest compactionTagFileRequest, IHeaderDictionary customHeaders = null);
    Task<ContractExecutionResult> SendTagFileNonDirect(CompactionTagFileRequest compactionTagFileRequest, IHeaderDictionary customHeaders = null);
  }
}
