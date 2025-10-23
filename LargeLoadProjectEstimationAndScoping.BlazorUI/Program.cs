using LargeLoadProjectEstimationAndScoping.BlazorUI;
using LargeLoadProjectEstimationAndScoping.BlazorUI.Features.VisualProposalDesigner.Services;
using LargeLoadProjectEstimationAndScoping.Infrastructure;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        var dev = true;

        builder.Services.AddDataCenterProjectDataService(dev);
        builder.Services.AddProjectDataService(dev);

        builder.Services.AddSingleton<GeoJsonDistanceService>();

        // HttpClient is scoped in WASM
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
        });

        // Make GISService scoped (NOT singleton)
        builder.Services.AddScoped<GISDataService>();

        await builder.Build().RunAsync();
    }
}