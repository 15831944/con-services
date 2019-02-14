﻿using System;

namespace VSS.TRex.Common.Exceptions
{
  public class TRexSubGridTreeException : Exception
  {
    public TRexSubGridTreeException(string message) : base(message)
    {
    }

    public TRexSubGridTreeException(string message, Exception E) : base(message, E)
    {
    }
  }
}
