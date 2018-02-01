﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.Common.Exceptions
{
  public class SingleFilterRequiredException : ServiceException
  {
    public SingleFilterRequiredException(HttpStatusCode statusCode, ContractExecutionResult contractExecution) :
    base(statusCode, contractExecution)
    { }
  }
}
