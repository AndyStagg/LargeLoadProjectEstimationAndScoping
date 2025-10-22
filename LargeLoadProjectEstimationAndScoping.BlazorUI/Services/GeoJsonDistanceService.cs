// PSEUDOCODE / PLAN (detailed):
// 1. Create interface IGeoJsonDistanceService with method:
//      (double miles, int points) CalculateTotalMiles(string geoJson)
// 2. Implement GeoJsonDistanceService:
//    - Parse the input string using JsonDocument.
//    - Determine root kind:
//       a) "FeatureCollection" -> iterate features -> extract geometry.
//       b) "Feature" -> take "geometry" property.
//       c) geometry object directly -> use it.
//    - For each geometry, handle types:
//       - "LineString": single array of coordinates -> extract sequential coordinate pairs.
//       - "MultiLineString": array of lines -> each line is array of coordinates.
//       - "GeometryCollection": iterate child geometries recursively.
//       - (Ignore other geometry types for polyline length; Polygon boundaries can be treated similarly if desired.)
//    - For each consecutive pair of coordinates (lon, lat), compute haversine distance in miles and sum.
//    - Return the total miles (double) and total point count (int).
// 3. Implement helper methods:
//    - IEnumerable<IEnumerable<(double lon, double lat)>> ExtractLineCollections(JsonElement geometryElement)
//      to yield sequences of points for each LineString found.
//    - double HaversineMiles((lon, lat) a, (lon, lat) b)
//      to compute great-circle distance using Earth's radius in miles.
// 4. Make implementation defensive: return (0,0) on invalid input or when no lines found.
// 5. Provide simple DI-friendly class (no registration code here; register in Program.cs as needed).
//
// The following file implements the above plan.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace LargeLoadProjectEstimationAndScoping.BlazorUI.Services
{
    public interface IGeoJsonDistanceService
    {
        /// <summary>
        /// Calculates the total length in miles and the total number of coordinate points
        /// for all LineString and MultiLineString geometries found in the provided GeoJSON.
        /// Supports GeoJSON FeatureCollection, Feature, and raw geometry objects.
        /// Returns (0.0, 0) for invalid or empty input.
        /// </summary>
        (double miles, int points) CalculateTotalMiles(string geoJson);
    }

    public class GeoJsonDistanceService : IGeoJsonDistanceService
    {
        // Mean Earth radius in miles
        private const double EarthRadiusMiles = 3958.7613;

        public (double miles, int points) CalculateTotalMiles(string geoJson)
        {
            if (string.IsNullOrWhiteSpace(geoJson)) return (0.0, 0);

            try
            {
                using var doc = JsonDocument.Parse(geoJson);
                var root = doc.RootElement;

                double totalMiles = 0.0;
                int totalPoints = 0;

                // If root is FeatureCollection -> iterate features
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("type", out var rootType) &&
                    rootType.ValueKind == JsonValueKind.String &&
                    string.Equals(rootType.GetString(), "FeatureCollection", StringComparison.OrdinalIgnoreCase))
                {
                    if (root.TryGetProperty("features", out var features) && features.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var feature in features.EnumerateArray())
                        {
                            if (feature.ValueKind != JsonValueKind.Object) continue;
                            if (feature.TryGetProperty("geometry", out var geometry))
                            {
                                var (miles, points) = SumGeometryMilesAndPoints(geometry);
                                totalMiles += miles;
                                totalPoints += points;
                            }
                        }
                    }
                }
                // If root is Feature -> use geometry property
                else if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("type", out var rt) &&
                         rt.ValueKind == JsonValueKind.String &&
                         string.Equals(rt.GetString(), "Feature", StringComparison.OrdinalIgnoreCase))
                {
                    if (root.TryGetProperty("geometry", out var geometry))
                    {
                        var (miles, points) = SumGeometryMilesAndPoints(geometry);
                        totalMiles += miles;
                        totalPoints += points;
                    }
                }
                // Otherwise assume it's a geometry object
                else
                {
                    var (miles, points) = SumGeometryMilesAndPoints(root);
                    totalMiles += miles;
                    totalPoints += points;
                }

                return (totalMiles, totalPoints);
            }
            catch
            {
                // On any parse error or unexpected structure, return (0,0) (fail-safe).
                return (0.0, 0);
            }
        }

        private (double miles, int points) SumGeometryMilesAndPoints(JsonElement geometry)
        {
            double sumMiles = 0.0;
            int sumPoints = 0;

            if (geometry.ValueKind != JsonValueKind.Object) return (0.0, 0);
            if (!geometry.TryGetProperty("type", out var typeProp) || typeProp.ValueKind != JsonValueKind.String)
                return (0.0, 0);

            var type = typeProp.GetString() ?? string.Empty;

            switch (type)
            {
                case "LineString":
                    foreach (var line in ExtractLineStringsFromLineString(geometry))
                    {
                        var (miles, points) = SumPointSequenceMilesAndCount(line);
                        sumMiles += miles;
                        sumPoints += points;
                    }
                    break;

                case "MultiLineString":
                    foreach (var line in ExtractLineStringsFromMultiLineString(geometry))
                    {
                        var (miles, points) = SumPointSequenceMilesAndCount(line);
                        sumMiles += miles;
                        sumPoints += points;
                    }
                    break;

                case "GeometryCollection":
                    if (geometry.TryGetProperty("geometries", out var geoms) && geoms.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var g in geoms.EnumerateArray())
                        {
                            var (miles, points) = SumGeometryMilesAndPoints(g);
                            sumMiles += miles;
                            sumPoints += points;
                        }
                    }
                    break;

                // Optionally, treat Polygon exterior ring as a line for length measurement
                case "Polygon":
                    // GeoJSON polygon coordinates: array of linear rings. Use exterior ring (first) to compute perimeter length.
                    if (geometry.TryGetProperty("coordinates", out var coords) && coords.ValueKind == JsonValueKind.Array)
                    {
                        // take first ring if present
                        var firstRing = coords.EnumerateArray().FirstOrDefault();
                        if (firstRing.ValueKind == JsonValueKind.Array)
                        {
                            var pts = ParseCoordinateArray(firstRing);
                            var (miles, points) = SumPointSequenceMilesAndCount(pts);
                            sumMiles += miles;
                            sumPoints += points;
                        }
                    }
                    break;

                default:
                    // Unsupported geometry type for polyline measurement
                    break;
            }

            return (sumMiles, sumPoints);
        }

        private static IEnumerable<List<(double lon, double lat)>> ExtractLineStringsFromLineString(JsonElement geom)
        {
            // LineString: "coordinates": [ [lon, lat], [lon, lat], ... ]
            if (geom.TryGetProperty("coordinates", out var coords) && coords.ValueKind == JsonValueKind.Array)
            {
                var pts = ParseCoordinateArray(coords);
                if (pts.Count >= 1) yield return pts;
            }
        }

        private static IEnumerable<List<(double lon, double lat)>> ExtractLineStringsFromMultiLineString(JsonElement geom)
        {
            // MultiLineString: "coordinates": [ [ [lon, lat], ... ], [ [lon, lat], ... ], ... ]
            if (geom.TryGetProperty("coordinates", out var coords) && coords.ValueKind == JsonValueKind.Array)
            {
                foreach (var line in coords.EnumerateArray())
                {
                    if (line.ValueKind != JsonValueKind.Array) continue;
                    var pts = ParseCoordinateArray(line);
                    if (pts.Count >= 1) yield return pts;
                }
            }
        }

        private static List<(double lon, double lat)> ParseCoordinateArray(JsonElement coordArray)
        {
            var list = new List<(double lon, double lat)>();

            if (coordArray.ValueKind != JsonValueKind.Array) return list;

            foreach (var item in coordArray.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Array) continue;

                // Expect [lon, lat] or [lon, lat, alt]
                var enumerator = item.EnumerateArray();
                if (!enumerator.MoveNext()) continue;
                if (enumerator.Current.ValueKind != JsonValueKind.Number) continue;
                var lon = enumerator.Current.GetDouble();

                if (!enumerator.MoveNext()) continue;
                if (enumerator.Current.ValueKind != JsonValueKind.Number) continue;
                var lat = enumerator.Current.GetDouble();

                list.Add((lon, lat));
            }

            return list;
        }

        private static (double miles, int points) SumPointSequenceMilesAndCount(List<(double lon, double lat)> points)
        {
            double sum = 0.0;
            for (int i = 1; i < points.Count; i++)
            {
                sum += HaversineMiles(points[i - 1], points[i]);
            }
            return (sum, points.Count);
        }

        private static double HaversineMiles((double lon, double lat) a, (double lon, double lat) b)
        {
            // Convert degrees to radians
            double lat1 = DegreesToRadians(a.lat);
            double lon1 = DegreesToRadians(a.lon);
            double lat2 = DegreesToRadians(b.lat);
            double lon2 = DegreesToRadians(b.lon);

            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;

            double sinDlat = Math.Sin(dLat / 2);
            double sinDlon = Math.Sin(dLon / 2);

            double hav = sinDlat * sinDlat + Math.Cos(lat1) * Math.Cos(lat2) * sinDlon * sinDlon;
            double c = 2 * Math.Atan2(Math.Sqrt(hav), Math.Sqrt(1 - hav));

            return EarthRadiusMiles * c;
        }

        private static double DegreesToRadians(double deg) => deg * (Math.PI / 180.0);
    }
}