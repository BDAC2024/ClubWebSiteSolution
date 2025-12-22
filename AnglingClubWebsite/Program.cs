using AnglingClubWebsite;
using AnglingClubWebsite.Authentication;
using AnglingClubWebsite.Pages;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CommunityToolkit.Mvvm.Messaging;
using Fishing.Client.Services;
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

builder.Services.AddHttpClient(Constants.HTTP_CLIENT_KEY)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration[Constants.API_ROOT_KEY] ?? ""))
                .AddHttpMessageHandler<AuthenticationHandler>();

// Infrastructure
builder.Services.AddScoped<IMessenger, WeakReferenceMessenger>();

// ViewModels
builder.Services.AddScoped<MainLayoutViewModel>();
builder.Services.AddScoped<IndexViewModel>();
builder.Services.AddScoped<DiaryViewModel>();
builder.Services.AddScoped<LoginViewModel>();
builder.Services.AddScoped<LogoutViewModel>();
builder.Services.AddScoped<NewsViewModel>();
builder.Services.AddScoped<WatersViewModel>();
builder.Services.AddScoped<MatchesViewModel>();

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
builder.Services.AddTransient<IRefDataService, RefDataService>();
builder.Services.AddTransient<IClubEventService, ClubEventService>();

builder.Services.AddAuthorizationCore();

// TODO Ang to Blazor Migration - services only needed during migration
builder.Services.AddScoped<HostBridge>();

var host = builder.Build();

// run initialization BEFORE the app starts rendering
var tokenStore = host.Services.GetRequiredService<IAuthTokenStore>();
await tokenStore.InitializeAsync();

await host.RunAsync();
