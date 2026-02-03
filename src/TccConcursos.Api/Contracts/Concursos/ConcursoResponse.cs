namespace TccConcursos.Api.Contracts.Concursos;

public sealed record ConcursoResponse(Guid Id, string Nome, DateOnly? DataProva);