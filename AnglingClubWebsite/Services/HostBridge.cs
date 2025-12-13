// TODO Ang to Blazor Migration - whole file only needed during migration
using AnglingClubShared.Models.Auth;
using Fishing.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Text.Json;
using System.Threading.Tasks;

public class HostBridge : IAsyncDisposable
{
  private readonly IJSRuntime _js;
  private readonly NavigationManager _nav;
  private readonly ILogger<HostBridge> _logger;
  private DotNetObjectReference<HostBridge>? _objRef;

  public event Action<string>? NavigateRequested;
  public event Action<AuthenticateResponse?, bool>? AuthUpdated;

  public HostBridge(IJSRuntime js, NavigationManager nav, ILogger<HostBridge> logger)
  {
    _js = js;
    _nav = nav;
    _logger = logger;
  }

  public async Task InitializeAsync()
  {
    _objRef = DotNetObjectReference.Create(this);
    await _js.InvokeVoidAsync("blazorHost.start", _objRef);
  }

  [JSInvokable]
  public Task HandleNavigate(string path)
  {
    Console.WriteLine($"HostBridge: Navigate to [{path}]");

    // Treat null/empty as "" so we stay on the "root" page of the Blazor app
    if (path is null)
      path = string.Empty;

    // Make sure it's a *relative* path (no leading slash)
    path = path.TrimStart('/');   // "news" instead of "/news"

    var currentUri = new Uri(_nav.Uri);
    var query = currentUri.Query; // e.g. "?embedded=true"

    string target;

    if (string.IsNullOrEmpty(path))
    {
      // Just "?embedded=true" → stays on the current base, e.g. "/new/?embedded=true"
      target = string.IsNullOrEmpty(query) ? string.Empty : query;
    }
    else
    {
      // "news?embedded=true" → resolved relative to the current base:
      // dev:  "/news?embedded=true"
      // prod: "/new/news?embedded=true"
      target = string.IsNullOrEmpty(query)
          ? path
          : $"{path}{query}";
    }
    
    _nav.NavigateTo(target, forceLoad: false);
    NavigateRequested?.Invoke(path);
    return Task.CompletedTask;
  }

  [JSInvokable]
  public Task HandleAuth(string? currentUser, bool rememberMe)
  {
    //Console.WriteLine($"HostBridge: Auth updated. currentUser is {(currentUser is null ? "null" : "set")}, rememberMe is {rememberMe}");
    //_logger.LogWarning($"HostBridge: Auth updated. currentUser is {currentUser}, rememberMe is {rememberMe}");

    AuthUpdated?.Invoke(JsonSerializer.Deserialize<AuthenticateResponse>(currentUser!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }), rememberMe);

    // You can also stash token in a service here
    return Task.CompletedTask;
  }

  public async ValueTask DisposeAsync()
  {
    _objRef?.Dispose();
    await Task.CompletedTask;
  }
}
