using LargeLoadProjectEstimationAndScoping.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LargeLoadProjectEstimationAndScoping.Core.Abstractions
{
    public interface IProjectDataService
    {
        Task<List<Project>> GetProjectsAsync();
        Task<List<Project>> GetProjectsAsync(Guid dataCenterId);
    }
}
