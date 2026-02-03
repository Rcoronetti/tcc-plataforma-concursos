namespace TccConcursos.Api.Contracts.SessoesEstudo;

public sealed record SessaoEstudoResponse(
    Guid Id,
    Guid TopicoId,
    DateTime Inicio,
    DateTime Fim,
    int Tipo,
    int? QuestoesTotal,
    int? QuestoesAcertos
);