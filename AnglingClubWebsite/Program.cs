using AnglingClubWebsite;
using AnglingClubWebsite.Authentication;
using AnglingClubWebsite.Pages;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using AnglingClubWebsite.SharedComponents.OnlyNeededWhilstMigrating;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CommunityToolkit.Mvvm.Messaging;
using Fishing.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.VisualBasic;
using Syncfusion.Blazor;
using Constants = AnglingClubWebsite.Constants;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var key = builder.Configuration["SyncfusionLicenseKey"];
if (!string.IsNullOrWhiteSpace(key))
{
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(key);
}

// Determine execution environment
var nav = builder.Services.BuildServiceProvider()
    .GetRequiredService<NavigationManager>();

bool isLocalhost =
    nav.BaseUri.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase) ||
    nav.BaseUri.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase) ||
    nav.BaseUri.StartsWith("http://127.0.0.1", StringComparison.OrdinalIgnoreCase);

bool isDevTunnel =
    nav.BaseUri.Contains("uks1.devtunnels.ms", StringComparison.OrdinalIgnoreCase);

bool isStaging =
    nav.BaseUri.Contains("purple-stone-0ae0b6b03-", StringComparison.OrdinalIgnoreCase);

string apiBaseUrl = "";

if (isDevTunnel)
{
    apiBaseUrl = builder.Configuration["ServerUrlDevTunnel"] ?? "";
}
else 
{
    if (isStaging)
    {
        apiBaseUrl = builder.Configuration["ServerUrlStaging"] ?? "";
    }
    else
    {
        apiBaseUrl = builder.Configuration["ServerUrl"] ?? "";
    }
}

//var uri = isDevTunnel ? new Uri(builder.Configuration["ServerUrlDevTunnel"] ?? "") : (new Uri(builder.Configuration[Constants.API_ROOT_KEY] ?? ""));
if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    throw new InvalidOperationException(
        "Configuration error: 'apiBaseUrl' is required but could not be determined.");
}

var apiUri = new Uri(apiBaseUrl);

builder.Services.AddHttpClient(Constants.HTTP_CLIENT_KEY)
                .ConfigureHttpClient(c => c.BaseAddress = apiUri)
                .AddHttpMessageHandler<AuthenticationHandler>();

// The app
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSyncfusionBlazor();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Auth
builder.Services.AddSingleton<IAuthTokenStore, AuthTokenStore>();
builder.Services.AddBlazoredLocalStorageAsSingleton();
builder.Services.AddBlazoredSessionStorageAsSingleton();
builder.Services.AddTransient<AuthenticationHandler>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddCascadingAuthenticationState();

// Infrastructure
builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();

// ViewModels
builder.Services.AddScoped<AppViewModel>();
builder.Services.AddScoped<MainLayoutViewModel>();
builder.Services.AddScoped<IndexViewModel>();
builder.Services.AddScoped<DiaryViewModel>();
builder.Services.AddScoped<LoginViewModel>();
builder.Services.AddScoped<LogoutViewModel>();
builder.Services.AddScoped<NewsViewModel>();
builder.Services.AddScoped<WatersViewModel>();
builder.Services.AddScoped<MatchesViewModel>();
builder.Services.AddScoped<SeasonSelectorViewModel>();


// Component ViewModels
builder.Services.AddScoped<AppLinkViewModel>();

// Services
builder.Services.AddSingleton<BrowserService>();
builder.Services.AddSingleton<IGlobalService, GlobalService>();
builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
builder.Services.AddSingleton<ICurrentUserService, CurrentUserService>();
builder.Services.AddTransient<IAppDialogService, AppDialogService>();
builder.Services.AddTransient<INavigationService, NavigationService>();
builder.Services.AddTransient<INewsService, NewsService>();
builder.Services.AddTransient<IWatersService, WatersService>();
builder.Services.AddScoped<IRefDataService, RefDataService>();
builder.Services.AddTransient<IClubEventService, ClubEventService>();
builder.Services.AddTransient<IMatchResultsService, MatchResultsService>();
builder.Services.AddTransient<IAboutService, AboutService>();

builder.Services.AddAuthorizationCore();

// TODO Ang to Blazor Migration - services only needed during migration
builder.Services.AddScoped<HostBridge>();
builder.Services.AddScoped<EmbeddedLayoutViewModel>();

var host = builder.Build();

// run initialization BEFORE the app starts rendering
var tokenStore = host.Services.GetRequiredService<IAuthTokenStore>();
await tokenStore.InitializeAsync();

await host.RunAsync();
