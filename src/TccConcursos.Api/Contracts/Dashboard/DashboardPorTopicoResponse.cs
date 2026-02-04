namespace TccConcursos.Api.Contracts.Dashboard;

public sealed record DashboardPorTopicoResponse(
    Guid TopicoId,
    Guid DisciplinaId,
    string TopicoNome,
    int TotalSessoes,
    int TotalMinutosEstudados,
    int TotalQuestoes,
    int TotalAcertos,
    double? TaxaAcertoPercentual
);