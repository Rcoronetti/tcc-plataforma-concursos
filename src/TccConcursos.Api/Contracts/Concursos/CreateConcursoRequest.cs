namespace TccConcursos.Api.Contracts.Concursos
{
    public sealed record CreateConcursoRequest(string Nome, DateOnly? DataProva);
}
