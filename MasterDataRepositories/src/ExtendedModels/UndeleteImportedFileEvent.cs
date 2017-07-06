﻿using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace Repositories.ExtendedModels
{
  public class UndeleteImportedFileEvent : IProjectEvent
  {
    public Guid ImportedFileUID { get; set; }

    public Guid ProjectUID { get; set; }

    public DateTime ActionUTC { get; set; }

    public DateTime ReceivedUTC { get; set; }
  }
}
