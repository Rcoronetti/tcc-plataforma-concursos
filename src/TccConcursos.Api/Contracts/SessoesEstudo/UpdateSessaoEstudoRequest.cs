namespace TccConcursos.Api.Contracts.SessoesEstudo;

public sealed record UpdateSessaoEstudoRequest(
    DateTime Inicio,
    DateTime Fim,
    int Tipo,
    int? QuestoesTotal,
    int? QuestoesAcertos
);