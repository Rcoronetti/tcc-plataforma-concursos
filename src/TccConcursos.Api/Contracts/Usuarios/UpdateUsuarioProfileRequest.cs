namespace TccConcursos.Api.Contracts.Usuarios;

public sealed record UpdateUsuarioProfileRequest(string Nome, string Endereco, string Telefone, string Bio, string FotoUrl);
