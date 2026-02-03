namespace TccConcursos.Api.Contracts.SessoesEstudo;

public sealed record CreateSessaoEstudoRequest(
    DateTime Inicio,
    DateTime Fim,
    int Tipo,
    int? QuestoesTotal,
    int? QuestoesAcertos
);