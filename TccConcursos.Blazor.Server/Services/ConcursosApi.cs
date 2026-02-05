namespace TccConcursos.Blazor.Server.Services;

public sealed class ConcursosApi
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ConcursosApi(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<ConcursoDto>> GetConcursosAsync(CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var data = await client.GetFromJsonAsync<List<ConcursoDto>>("concursos", ct);
        return data ?? new List<ConcursoDto>();
    }

    public async Task<ConcursoDto> CreateConcursoAsync(CreateConcursoRequest request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");

        var resp = await client.PostAsJsonAsync("concursos", request, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Erro ao criar concurso. Status={(int)resp.StatusCode}. Body={body}");
        }

        var created = await resp.Content.ReadFromJsonAsync<ConcursoDto>(cancellationToken: ct);
        return created ?? throw new InvalidOperationException("API não retornou o concurso criado.");
    }

    public sealed record ConcursoDto(Guid Id, string Nome, DateOnly? DataProva);

    public sealed record CreateConcursoRequest(string Nome, DateOnly? DataProva);
}