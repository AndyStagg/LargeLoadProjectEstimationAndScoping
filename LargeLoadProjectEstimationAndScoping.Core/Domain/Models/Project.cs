using System;

namespace LargeLoadProjectEstimationAndScoping.Core.Domain.Models
{
    public class Project
    {
        public Guid DataCenterId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string OrderNum { get; set; } = "";
        public string FuncNum { get; set; } = "";
        public ProjectStatus Status { get; set; } = ProjectStatus.Active;
    }
}
