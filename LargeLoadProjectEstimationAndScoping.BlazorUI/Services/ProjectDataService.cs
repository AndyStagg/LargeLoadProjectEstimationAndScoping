using LargeLoadProjectEstimationAndScoping.BlazorUI.Models;

namespace LargeLoadProjectEstimationAndScoping.BlazorUI.Services
{
    public class ProjectDataService
    {
        private readonly List<Project> _projects;

        public ProjectDataService()
        {
            _projects = new()
            {
                new Project {
                    DataCenterId = Guid.Parse("F1C669E3-BE10-466C-9CE7-418DD35804A3"),
                    Name = "T.0001649", Description="PEASE - REPL BANK 2", OrderNum="74004890", FuncNum="ETS.19.12345", Status=ProjectStatus.Active,
                    //Facilities = new List<Facility>{
                    //    new Facility{ Name="Bank 2", FuncNum="ETS.19.12345", Type="Substation"},
                    //    new Facility{ Name="Control Room", FuncNum="ETS.19.12345", Type="Building"}
                    //}
                },
                new Project {
                    DataCenterId = Guid.Parse("C659E22C-5049-4474-B8E9-E2A7F56C21F1"),
                    Name = "T.0002930", Description="PEASE SUB INSTALL DDT TRANSMITTER", OrderNum="74008360", FuncNum="ETS.19.12345", Status=ProjectStatus.Active,
                    //Facilities = new List<Facility>{
                    //    new Facility{ Name="Pease Sub A", FuncNum="ETS.19.23456", Type="Substation"}
                    //}
                },
                //new Project {
                //    Name = "T.0003999", Description="Sample Inactive Project", OrderNum="74009999", FuncNum="ETS.19.54321", Status=ProjectStatus.Inactive,
                //    Facilities = new List<Facility>{
                //        new Facility{ Name="Yard 1", FuncNum="ETS.19.54321", Type="Yard"}
                //    }
                //}
            };
        }

        public async Task<List<Project>> GetProjectsAsync() => await Task.FromResult(_projects);

        public async Task<List<Project>> GetProjectsAsync(Guid dataCenterId) =>
            await Task.FromResult(_projects.Where(p => p.DataCenterId == dataCenterId).ToList());
    }
}
