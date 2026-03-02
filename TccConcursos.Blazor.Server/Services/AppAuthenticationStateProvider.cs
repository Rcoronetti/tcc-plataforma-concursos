using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace TccConcursos.Blazor.Server.Services;

public sealed class AppAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));
    private AuthenticationState _currentState = Anonymous;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(_currentState);

    public bool Login(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, userName.Trim()),
            new Claim(ClaimTypes.Role, "Concurseiro")
        ],
        authenticationType: "AppAuth");

        _currentState = new AuthenticationState(new ClaimsPrincipal(identity));
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
        return true;
    }

    public void Logout()
    {
        _currentState = Anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }
}
