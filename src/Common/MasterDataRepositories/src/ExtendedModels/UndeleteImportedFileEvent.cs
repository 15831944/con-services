﻿using System;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.Repositories.ExtendedModels
{
  public class UndeleteImportedFileEvent : IProjectEvent
  {
    public Guid ImportedFileUID { get; set; }

    public Guid ProjectUID { get; set; }

    public DateTime ActionUTC { get; set; }
  }
}
