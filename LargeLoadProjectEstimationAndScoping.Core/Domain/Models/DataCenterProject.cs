using System;

namespace LargeLoadProjectEstimationAndScoping.Core.Domain.Models
{
    public class DataCenterProject
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public DataCenterProject()
        {
            Id = Guid.NewGuid();
            Name = "";
        }
    }
}
