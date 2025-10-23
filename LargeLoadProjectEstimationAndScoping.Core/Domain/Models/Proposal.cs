using System;

namespace LargeLoadProjectEstimationAndScoping.Core.Domain.Models
{
    public class Proposal
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Proposal()
        {
            Id = Guid.NewGuid();
            Name = "";
        }
    }
}
