using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace TccConcursos.Blazor.Server.Services;

public sealed class AppAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly ConcursosApi _api;
    private AuthenticationState _currentState = Anonymous;
    private Guid? _currentUserId;

    public AppAuthenticationStateProvider(ConcursosApi api)
    {
        _api = api;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(_currentState);

    public async Task<(bool Success, string? Error)> LoginAsync(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Informe login e senha.");
        }

        var result = await _api.LoginUsuarioAsync(new ConcursosApi.LoginUsuarioRequest(login, password));
        if (!result.Success || result.Data is null)
        {
            return (false, result.ErrorMessage ?? "Credenciais inválidas. Faça seu cadastro se for o primeiro acesso.");
        }

        SetAuthenticatedState(result.Data);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(string name, string cpf, string email, string password)
    {
        var result = await _api.RegisterUsuarioAsync(new ConcursosApi.RegisterUsuarioRequest(name, cpf, email, password));
        return result.Success
            ? (true, null)
            : (false, result.ErrorMessage ?? "Não foi possível concluir o cadastro.");
    }

    public async Task<UserProfile?> GetCurrentUserProfileAsync()
    {
        if (_currentUserId is null)
        {
            return null;
        }

        var profile = await _api.GetUsuarioProfileAsync(_currentUserId.Value);
        if (profile is null)
        {
            return null;
        }

        return new UserProfile(
            profile.Id,
            profile.Nome,
            profile.Email,
            profile.Cpf,
            profile.Endereco,
            profile.Telefone,
            profile.Bio,
            profile.FotoUrl);
    }

    public async Task<(bool Success, string? Error)> UpdateCurrentUserProfileAsync(UserProfile profile)
    {
        if (_currentUserId is null)
        {
            return (false, "Usuário não autenticado.");
        }

        var result = await _api.UpdateUsuarioProfileAsync(
            _currentUserId.Value,
            new ConcursosApi.UpdateUsuarioProfileRequest(
                profile.Name,
                profile.Address,
                profile.Phone,
                profile.Bio,
                profile.PhotoUrl));

        if (!result.Success || result.Data is null)
        {
            return (false, result.ErrorMessage ?? "Não foi possível atualizar o perfil.");
        }

        SetAuthenticatedState(result.Data);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangeCurrentUserPasswordAsync(string currentPassword, string newPassword)
    {
        if (_currentUserId is null)
        {
            return (false, "Usuário não autenticado.");
        }

        var result = await _api.ChangeUsuarioPasswordAsync(
            _currentUserId.Value,
            new ConcursosApi.ChangeUsuarioPasswordRequest(currentPassword, newPassword));

        return result.Success
            ? (true, null)
            : (false, result.ErrorMessage ?? "Não foi possível alterar a senha.");
    }

    public void Logout()
    {
        _currentUserId = null;
        _currentState = Anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    private void SetAuthenticatedState(ConcursosApi.UsuarioProfileDto user)
    {
        _currentUserId = user.Id;

        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, user.Nome),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("cpf", user.Cpf),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, "Concurseiro")
        ],
        authenticationType: "AppAuth");

        _currentState = new AuthenticationState(new ClaimsPrincipal(identity));
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    public sealed record UserProfile(
        Guid Id,
        string Name,
        string Email,
        string Cpf,
        string Address,
        string Phone,
        string Bio,
        string PhotoUrl)
    {
        public Guid Id { get; set; } = Id;
        public string Name { get; set; } = Name;
        public string Email { get; set; } = Email;
        public string Cpf { get; set; } = Cpf;
        public string Address { get; set; } = Address;
        public string Phone { get; set; } = Phone;
        public string Bio { get; set; } = Bio;
        public string PhotoUrl { get; set; } = PhotoUrl;
    }
}
