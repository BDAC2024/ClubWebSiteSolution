using AnglingClubShared.Models.Auth;
using AnglingClubWebsite.Extensions;
using AnglingClubWebsite.Models;
using Blazored.LocalStorage;
using Blazored.SessionStorage;

public interface IAuthTokenStore
{
    AuthenticateResponse? Current { get; set; }
    Task InitializeAsync();
}

public class AuthTokenStore : IAuthTokenStore
{
    private readonly ISessionStorageService _sessionStorage;
    private readonly ILocalStorageService _localStorage;

    public AuthenticateResponse? Current { get; set; }

    public AuthTokenStore(
        ISessionStorageService sessionStorage,
        ILocalStorageService localStorage)
    {
        _sessionStorage = sessionStorage;
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        // Check session storage first then local storage
        Current =
            await _sessionStorage.ReadEncryptedItem<AuthenticateResponse>(Constants.AUTH_KEY)
            ?? await _localStorage.ReadEncryptedItem<AuthenticateResponse>(Constants.AUTH_KEY);
    }
}
