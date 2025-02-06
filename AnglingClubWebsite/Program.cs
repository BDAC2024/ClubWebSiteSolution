using AnglingClubWebsite;
using AnglingClubWebsite.Authentication;
using AnglingClubWebsite.Pages;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using Blazored.LocalStorage;
using CommunityToolkit.Mvvm.Messaging;
using Fishing.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.VisualBasic;
using Syncfusion.Blazor;
using Constants = AnglingClubWebsite.Constants;

// Another test after transfer

// 28.*.*
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzY2NDY5N0AzMjM4MmUzMDJlMzBlcTNkaVU0d1kyQXZMbzRHZnJOMGFBMngzSUs2TUU1UkU2LzYxcFZ5T2JJPQ==");

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSyncfusionBlazor();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Auth
builder.Services.AddBlazoredLocalStorageAsSingleton();
builder.Services.AddTransient<AuthenticationHandler>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddTransient<AnonymousRoutes>();

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

// Component ViewModels
builder.Services.AddScoped<AppLinkViewModel>();

// Services
builder.Services.AddSingleton<BrowserService>();
builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
builder.Services.AddSingleton<ICurrentUserService, CurrentUserService>();
builder.Services.AddTransient<IAppDialogService, AppDialogService>();
builder.Services.AddTransient<INavigationService, NavigationService>();
builder.Services.AddTransient<INewsService, NewsService>();
builder.Services.AddTransient<IWatersService, WatersService>();

builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
