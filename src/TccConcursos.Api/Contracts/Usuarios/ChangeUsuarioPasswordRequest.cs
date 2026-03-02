namespace TccConcursos.Api.Contracts.Usuarios;

public sealed record ChangeUsuarioPasswordRequest(string SenhaAtual, string NovaSenha);
