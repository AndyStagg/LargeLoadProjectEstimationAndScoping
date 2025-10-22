namespace LargeLoadProjectEstimationAndScoping.BlazorUI.Models
{
    public class Project
    {
        public Guid DataCenterId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string OrderNum { get; set; } = "";
        public string FuncNum { get; set; } = "";
        public ProjectStatus Status { get; set; } = ProjectStatus.Active;

        //public List<Facility> Facilities { get; set; } = new();
    }

    //public class Facility
    //{
    //    public string Name { get; set; } = "";
    //    public string FuncNum { get; set; } = "";
    //    public string Type { get; set; } = "";
    //}

    public enum ProjectStatus { Active, Inactive }
}
