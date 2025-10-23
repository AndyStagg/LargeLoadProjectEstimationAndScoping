using LargeLoadProjectEstimationAndScoping.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LargeLoadProjectEstimationAndScoping.Core.Abstractions
{
    public interface IDataCenterProjectDataService
    {
        Task<DataCenterProject?> GetDataCenterPojectAsync(Guid id);
        Task<DataCenterProject?> GetDataCenterProjectAsync(string name);
        Task<List<DataCenterProject>> GetDataCenterProjectsAsync();
    }
}
