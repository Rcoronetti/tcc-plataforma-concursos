namespace TccConcursos.Api.Contracts.Topicos;

public sealed record TopicoResponse(Guid Id, Guid DisciplinaId, string Nome);