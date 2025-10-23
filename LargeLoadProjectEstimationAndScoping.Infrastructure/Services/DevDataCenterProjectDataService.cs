using LargeLoadProjectEstimationAndScoping.Core.Abstractions;
using LargeLoadProjectEstimationAndScoping.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LargeLoadProjectEstimationAndScoping.Infrastructure.Services
{
    public class DevDataCenterProjectDataService : IDataCenterProjectDataService
    {
        private readonly List<DataCenterProject> _dataCenters;

        public DevDataCenterProjectDataService()
        {
            _dataCenters = new List<DataCenterProject>();

            _dataCenters.Add(new DataCenterProject { Name = "San Jose A DC#1", Id = Guid.Parse("F1C669E3-BE10-466C-9CE7-418DD35804A3") });
            _dataCenters.Add(new DataCenterProject { Name = "San Jose B DC#2", Id = Guid.Parse("C659E22C-5049-4474-B8E9-E2A7F56C21F1") });
        }

        public async Task<List<DataCenterProject>> GetDataCenterProjectsAsync() =>
            await Task.FromResult(_dataCenters);

        public async Task<DataCenterProject?> GetDataCenterProjectAsync(string name) =>
            await Task.FromResult(_dataCenters.FirstOrDefault(dc => dc.Name == name));

        public async Task<DataCenterProject?> GetDataCenterPojectAsync(Guid id) =>
            await Task.FromResult(_dataCenters.FirstOrDefault(dc => dc.Id == id));
    }
}
