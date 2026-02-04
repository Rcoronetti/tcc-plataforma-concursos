namespace TccConcursos.Api.Contracts.Dashboard;

public sealed record DashboardPorConcursoResponse(
    Guid ConcursoId,
    string ConcursoNome,
    int TotalSessoes,
    int TotalMinutosEstudados,
    int TotalQuestoes,
    int TotalAcertos,
    double? TaxaAcertoPercentual
);