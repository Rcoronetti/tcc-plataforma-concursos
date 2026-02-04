namespace TccConcursos.Api.Contracts.Dashboard;

public sealed record DashboardResumoResponse(
    int TotalSessoes,
    int TotalMinutosEstudados,
    int TotalQuestoes,
    int TotalAcertos,
    double? TaxaAcertoPercentual
);