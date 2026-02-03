namespace TccConcursos.Api.Contracts.Disciplinas;

public sealed record DisciplinaResponse(Guid Id, Guid ConcursoId, string Nome);