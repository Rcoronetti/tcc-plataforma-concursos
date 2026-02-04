namespace TccConcursos.Api.Contracts.Dashboard;

public sealed record DashboardPorDisciplinaResponse(
    Guid DisciplinaId,
    string DisciplinaNome,
    int TotalSessoes,
    int TotalMinutosEstudados,
    int TotalQuestoes,
    int TotalAcertos,
    double? TaxaAcertoPercentual
);