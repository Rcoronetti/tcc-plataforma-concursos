namespace TccConcursos.Api.Contracts.Concursos;

public sealed record UpdateConcursoRequest(string Nome, DateOnly? DataProva);