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

        UsersByEmail[normalizedEmail] = new RegisteredUser(normalizedName, normalizedCpf, normalizedEmail, password);
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

    private sealed record RegisteredUser(string Name, string Cpf, string Email, string Password);
}
