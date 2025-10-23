using LargeLoadProjectEstimationAndScoping.BlazorUI.Features.VisualProposalDesigner.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LargeLoadProjectEstimationAndScoping.BlazorUI.Features.VisualProposalDesigner.Utils
{
    public static class GeoJsonBuilder
    {
        private static readonly JsonSerializerOptions _opts = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string BuildFacilityFences(ElectricFacilityFence[] fences)
        {
            var features = new List<object>();

            foreach (var f in fences ?? Array.Empty<ElectricFacilityFence>())
            {
                if (f?.Polygons == null) continue;

                foreach (var poly in f.Polygons)
                {
                    if (poly?.Points == null || poly.Points.Count < 3) continue;

                    // GeoJSON requires [lon, lat]
                    var ring = poly.Points.Select(p => new[] { p.Longitude, p.Latitude }).ToList();

                    // Ensure closed ring
                    if (ring.Count > 0 && (ring[0][0] != ring[^1][0] || ring[0][1] != ring[^1][1]))
                        ring.Add(new[] { ring[0][0], ring[0][1] });

                    features.Add(new
                    {
                        type = "Feature",
                        properties = new { f.Attributes?.name_ets, layer = "ElectricFacilityFence" },
                        geometry = new
                        {
                            type = "Polygon",
                            coordinates = new List<List<double[]>> { ring }
                        }
                    });
                }
            }

            var fc = new { type = "FeatureCollection", features };
            return JsonSerializer.Serialize(fc, _opts);
        }

        public static string BuildLineSegments<TAttr>(
            IEnumerable<(string layerName, string? name, string? extra1, List<Polyline>? lines)> groups,
            Func<TAttr, (string? name, string? extra1)> attrPicker)
        {
            var features = new List<object>();

            foreach (var g in groups)
            {
                if (g.lines == null) continue;

                foreach (var line in g.lines)
                {
                    if (line?.Points == null || line.Points.Count < 2) continue;

                    var coords = line.Points.Select(p => new[] { p.Longitude, p.Latitude }).ToList();

                    features.Add(new
                    {
                        type = "Feature",
                        properties = new { tline_nm = g.name, extra = g.extra1, layer = g.layerName },
                        geometry = new { type = "LineString", coordinates = coords }
                    });
                }
            }

            var fc = new { type = "FeatureCollection", features };
            return JsonSerializer.Serialize(fc, _opts);
        }

        public static (double west, double south, double east, double north) ComputeBounds(ETGISData data)
        {
            double west = double.PositiveInfinity, south = double.PositiveInfinity;
            double east = double.NegativeInfinity, north = double.NegativeInfinity;

            void Acc(double lon, double lat)
            {
                if (lon < west) west = lon;
                if (lat < south) south = lat;
                if (lon > east) east = lon;
                if (lat > north) north = lat;
            }

            foreach (var f in data.ElectricFacilityFence ?? new List<ElectricFacilityFence>())
                foreach (var poly in f.Polygons ?? new List<Polygon>())
                    foreach (var p in poly.Points ?? new List<Point>())
                        Acc(p.Longitude, p.Latitude);

            foreach (var u in data.UGLineSegment ?? new List<UGLineSegment>())
                foreach (var l in u.Polylines ?? new List<Polyline>())
                    foreach (var p in l.Points ?? new List<Point>())
                        Acc(p.Longitude, p.Latitude);

            foreach (var o in data.OHLineSegment ?? new List<OHLineSegment>())
                foreach (var l in o.Polylines ?? new List<Polyline>())
                    foreach (var p in l.Points ?? new List<Point>())
                        Acc(p.Longitude, p.Latitude);

            // Fallback to RequestCriteria if data had no points
            if (double.IsInfinity(west) && data.RequestCriteria is not null)
                return (data.RequestCriteria.WesternBoundary, data.RequestCriteria.SouthernBoundary,
                        data.RequestCriteria.EasternBoundary, data.RequestCriteria.NorthernBoundary);

            return (west, south, east, north);
        }
    }
}
