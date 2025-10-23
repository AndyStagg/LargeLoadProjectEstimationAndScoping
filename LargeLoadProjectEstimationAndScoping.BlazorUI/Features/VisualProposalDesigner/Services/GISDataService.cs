using LargeLoadProjectEstimationAndScoping.BlazorUI.Features.VisualProposalDesigner.Models;
using System.Text.Json;

namespace LargeLoadProjectEstimationAndScoping.BlazorUI.Features.VisualProposalDesigner.Services
{
    public class GISDataService
    {
        private readonly HttpClient _http;
        private ETGISData? _data;

        public GISDataService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ETGISData> GetDataAsync()
        {
            if (_data is not null)
                return _data;

            // loads from wwwroot/data/ETGIS_Data.json
            var json = await _http.GetStringAsync("data/ETGIS_Data.json");
            _data = JsonSerializer.Deserialize<ETGISData>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new ETGISData();

            return _data;
        }
    }
}
