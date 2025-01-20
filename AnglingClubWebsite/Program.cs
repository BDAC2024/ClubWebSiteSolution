using AnglingClubWebsite;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Syncfusion.Blazor;

// 28.*.*
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzY2NDY5N0AzMjM4MmUzMDJlMzBlcTNkaVU0d1kyQXZMbzRHZnJOMGFBMngzSUs2TUU1UkU2LzYxcFZ5T2JJPQ==");

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSyncfusionBlazor();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
