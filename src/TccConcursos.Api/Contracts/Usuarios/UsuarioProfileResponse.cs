namespace TccConcursos.Api.Contracts.Usuarios;

public sealed record UsuarioProfileResponse(Guid Id, string Nome, string Email, string Cpf, string Endereco, string Telefone, string Bio, string FotoUrl);
