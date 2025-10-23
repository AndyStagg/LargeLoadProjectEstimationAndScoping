using LargeLoadProjectEstimationAndScoping.Core.Abstractions;
using LargeLoadProjectEstimationAndScoping.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LargeLoadProjectEstimationAndScoping.Infrastructure.Services
{
    public class DevProjectDataService : IProjectDataService
    {
        private readonly List<Project> _projects;

        public DevProjectDataService()
        {
            _projects = new()
            {
                new Project {
                    DataCenterId = Guid.Parse("F1C669E3-BE10-466C-9CE7-418DD35804A3"),
                    Name = "T.0001649", Description="PEASE - REPL BANK 2", OrderNum="74004890", FuncNum="ETS.19.12345", Status=ProjectStatus.Active,
                },
                new Project {
                    DataCenterId = Guid.Parse("C659E22C-5049-4474-B8E9-E2A7F56C21F1"),
                    Name = "T.0002930", Description="PEASE SUB INSTALL DDT TRANSMITTER", OrderNum="74008360", FuncNum="ETS.19.12345", Status=ProjectStatus.Active,
                },
            };
        }

        public async Task<List<Project>> GetProjectsAsync() => await Task.FromResult(_projects);

        public async Task<List<Project>> GetProjectsAsync(Guid dataCenterId) =>
            await Task.FromResult(_projects.Where(p => p.DataCenterId == dataCenterId).ToList());
    }
}
