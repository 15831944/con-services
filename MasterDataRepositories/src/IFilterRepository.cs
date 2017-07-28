using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.Repositories
{
  public interface IFilterRepository
  {
    Task<IEnumerable<Filter>> GetFiltersForProject(string projectUid);
    Task<Filter> GetFilter(string filterUid);
   
    Task<int> StoreEvent(IFilterEvent evt);
  }
}