namespace TccConcursos.Api.Contracts.Usuarios;

public sealed record RegisterUsuarioRequest(string Nome, string Cpf, string Email, string Senha);
