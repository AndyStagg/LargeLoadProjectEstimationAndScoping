namespace LargeLoadProjectEstimationAndScoping.BlazorUI.Features.VisualProposalDesigner.Models
{
    public class ETGISData
    {
        public RequestCriteria RequestCriteria { get; set; }
        public List<object> ErrorLayers { get; set; }
        public List<ElectricFacilityFence> ElectricFacilityFence { get; set; }
        public List<UGLineSegment> UGLineSegment { get; set; }
        public List<OHLineSegment> OHLineSegment { get; set; }
    }

    public class RequestCriteria
    {
        public double SouthernBoundary { get; set; }
        public double NorthernBoundary { get; set; }
        public double WesternBoundary { get; set; }
        public double EasternBoundary { get; set; }
    }

    public class ElectricFacilityFence
    {
        public string DisplayFieldName { get; set; }
        public ElectricFacilityFenceAttributes Attributes { get; set; }
        public List<Polygon> Polygons { get; set; }
    }

    public class ElectricFacilityFenceAttributes
    {
        public string name_ets { get; set; }
    }

    public class Polygon
    {
        public List<Point> Points { get; set; }
    }

    public class Point
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class UGLineSegment
    {
        public string DisplayFieldName { get; set; }
        public UGLineSegmentAttributes Attributes { get; set; }
        public List<Polyline> Polylines { get; set; }
    }

    public class UGLineSegmentAttributes
    {
        public string tline_nm { get; set; }
        public string nominal_voltage { get; set; }
    }

    public class Polyline
    {
        public List<Point> Points { get; set; }
    }

    public class OHLineSegment
    {
        public string DisplayFieldName { get; set; }
        public OHLineSegmentAttributes Attributes { get; set; }
        public List<Polyline> Polylines { get; set; }
    }

    public class OHLineSegmentAttributes
    {
        public string tline_nm { get; set; }
        public string status { get; set; }
        public string ratedkv { get; set; }
    }
}
