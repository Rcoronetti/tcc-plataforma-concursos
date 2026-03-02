using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Authorization;

namespace TccConcursos.Blazor.Server.Services;

public sealed class AppAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));
    private static readonly Dictionary<string, RegisteredUser> UsersByEmail = new(StringComparer.OrdinalIgnoreCase);
    private AuthenticationState _currentState = Anonymous;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(_currentState);

    public bool Login(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var normalizedLogin = login.Trim();

        var user = UsersByEmail.Values.FirstOrDefault(u =>
            u.Email.Equals(normalizedLogin, StringComparison.OrdinalIgnoreCase) ||
            u.Cpf.Equals(OnlyDigits(normalizedLogin), StringComparison.Ordinal));

        if (user is null || !user.Password.Equals(password, StringComparison.Ordinal))
        {
            return false;
        }

        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("cpf", user.Cpf),
            new Claim(ClaimTypes.Role, "Concurseiro")
        ],
        authenticationType: "AppAuth");

        _currentState = new AuthenticationState(new ClaimsPrincipal(identity));
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
        return true;
    }

    public bool Register(string name, string cpf, string email, string password, out string? error)
    {
        error = null;

        var normalizedName = name.Trim();
        var normalizedCpf = OnlyDigits(cpf);
        var normalizedEmail = email.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            error = "Informe o nome completo.";
            return false;
        }

        if (!IsValidCpf(normalizedCpf))
        {
            error = "CPF inválido.";
            return false;
        }

        if (!IsValidEmail(normalizedEmail))
        {
            error = "E-mail inválido.";
            return false;
        }

        if (!IsStrongPassword(password))
        {
            error = "A senha deve ter no mínimo 8 caracteres, com maiúscula, minúscula, número e símbolo.";
            return false;
        }

        if (UsersByEmail.ContainsKey(normalizedEmail))
        {
            error = "Já existe cadastro com este e-mail.";
            return false;
        }

        if (UsersByEmail.Values.Any(u => u.Cpf.Equals(normalizedCpf, StringComparison.Ordinal)))
        {
            error = "Já existe cadastro com este CPF.";
            return false;
        }

        UsersByEmail[normalizedEmail] = new RegisteredUser
        {
            Name = normalizedName,
            Cpf = normalizedCpf,
            Email = normalizedEmail,
            Password = password,
            Address = string.Empty,
            Phone = string.Empty,
            Bio = string.Empty,
            PhotoUrl = string.Empty
        };
        return true;
    }

    public UserProfile? GetCurrentUserProfile()
    {
        var user = GetCurrentUser();
        if (user is null)
        {
            return null;
        }

        return new UserProfile(
            user.Name,
            user.Email,
            user.Cpf,
            user.Address,
            user.Phone,
            user.Bio,
            user.PhotoUrl);
    }

    public bool UpdateCurrentUserProfile(UserProfile profile, out string? error)
    {
        error = null;
        var user = GetCurrentUser();

        if (user is null)
        {
            error = "Usuário não autenticado.";
            return false;
        }

        var normalizedName = profile.Name.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            error = "Informe o nome completo.";
            return false;
        }

        user.Name = normalizedName;
        user.Address = profile.Address.Trim();
        user.Phone = profile.Phone.Trim();
        user.Bio = profile.Bio.Trim();
        user.PhotoUrl = profile.PhotoUrl.Trim();

        RefreshLoggedUserClaims(user);
        return true;
    }

    public bool ChangeCurrentUserPassword(string currentPassword, string newPassword, out string? error)
    {
        error = null;
        var user = GetCurrentUser();

        if (user is null)
        {
            error = "Usuário não autenticado.";
            return false;
        }

        if (!user.Password.Equals(currentPassword, StringComparison.Ordinal))
        {
            error = "A senha atual está incorreta.";
            return false;
        }

        if (!IsStrongPassword(newPassword))
        {
            error = "A nova senha deve ter no mínimo 8 caracteres, com maiúscula, minúscula, número e símbolo.";
            return false;
        }

        if (newPassword.Equals(currentPassword, StringComparison.Ordinal))
        {
            error = "A nova senha deve ser diferente da senha atual.";
            return false;
        }

        user.Password = newPassword;
        return true;
    }

    public void Logout()
    {
        _currentState = Anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    private static bool IsValidEmail(string email)
        => Regex.IsMatch(email, "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$", RegexOptions.CultureInvariant);

    private static bool IsStrongPassword(string password)
        => password.Length >= 8
           && password.Any(char.IsUpper)
           && password.Any(char.IsLower)
           && password.Any(char.IsDigit)
           && password.Any(ch => !char.IsLetterOrDigit(ch));

    private static string OnlyDigits(string value)
        => new(value.Where(char.IsDigit).ToArray());

    private static bool IsValidCpf(string cpf)
    {
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
        {
            return false;
        }

        var numbers = cpf.Select(c => c - '0').ToArray();

        var firstDigit = CalculateCpfDigit(numbers, 9, 10);
        if (numbers[9] != firstDigit)
        {
            return false;
        }

        var secondDigit = CalculateCpfDigit(numbers, 10, 11);
        return numbers[10] == secondDigit;
    }

    private static int CalculateCpfDigit(int[] numbers, int length, int weightStart)
    {
        var sum = 0;
        for (var i = 0; i < length; i++)
        {
            sum += numbers[i] * (weightStart - i);
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    private RegisteredUser? GetCurrentUser()
    {
        var email = _currentState.User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return UsersByEmail.TryGetValue(email, out var user) ? user : null;
    }

    private void RefreshLoggedUserClaims(RegisteredUser user)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("cpf", user.Cpf),
            new Claim(ClaimTypes.Role, "Concurseiro")
        ],
        authenticationType: "AppAuth");

        _currentState = new AuthenticationState(new ClaimsPrincipal(identity));
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    public sealed record UserProfile(
        string Name,
        string Email,
        string Cpf,
        string Address,
        string Phone,
        string Bio,
        string PhotoUrl);

    private sealed class RegisteredUser
    {
        public required string Name { get; set; }
        public required string Cpf { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
    }
}
