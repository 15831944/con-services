﻿using System;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Webtools
{
  public static class SwitchableGridContext
  {
    public static StorageMutability switchableMutability = StorageMutability.Mutable;

    private static ISiteModels[] SwitchableSiteModelsContexts = new ISiteModels[Enum.GetNames(typeof(StorageMutability)).Length];

    public static ISiteModels SwitchableSiteModelsContext()
    {
      return SwitchableSiteModelsContexts[(int) switchableMutability] ??= new SiteModels.SiteModels();
    }
  }
}
