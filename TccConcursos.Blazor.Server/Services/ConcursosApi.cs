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

    public async Task<ConcursoDto?> UpdateConcursoAsync(Guid id, UpdateConcursoRequest request)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PutAsJsonAsync($"concursos/{id}", request);

        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ConcursoDto>();
    }

    public async Task<bool> DeleteConcursoAsync(Guid id)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.DeleteAsync($"concursos/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<DisciplinaDto>> GetDisciplinasAsync(Guid concursoId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var data = await client.GetFromJsonAsync<List<DisciplinaDto>>($"concursos/{concursoId}/disciplinas", ct);
        return data ?? new List<DisciplinaDto>();
    }

    public async Task<DisciplinaDto> CreateDisciplinaAsync(Guid concursoId, CreateDisciplinaRequest request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");

        var resp = await client.PostAsJsonAsync($"concursos/{concursoId}/disciplinas", request, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Erro ao criar disciplina. Status={(int)resp.StatusCode}. Body={body}");
        }

        var created = await resp.Content.ReadFromJsonAsync<DisciplinaDto>(cancellationToken: ct);
        return created ?? throw new InvalidOperationException("API não retornou a disciplina criada.");
    }

    public async Task<DisciplinaDto?> UpdateDisciplinaAsync(Guid concursoId, Guid disciplinaId, UpdateDisciplinaRequest request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");

        var resp = await client.PutAsJsonAsync($"concursos/{concursoId}/disciplinas/{disciplinaId}", request, ct);
        if (!resp.IsSuccessStatusCode) return null;

        return await resp.Content.ReadFromJsonAsync<DisciplinaDto>(cancellationToken: ct);
    }

    public async Task<bool> DeleteDisciplinaAsync(Guid concursoId, Guid disciplinaId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var resp = await client.DeleteAsync($"concursos/{concursoId}/disciplinas/{disciplinaId}", ct);
        return resp.IsSuccessStatusCode;
    }

    // DTOs
    public sealed record ConcursoDto(Guid Id, string Nome, DateOnly? DataProva);
    public sealed record CreateConcursoRequest(string Nome, DateOnly? DataProva);
    public sealed record UpdateConcursoRequest(string Nome, DateOnly? DataProva);
    public sealed record DisciplinaDto(Guid Id, Guid ConcursoId, string Nome);
    public sealed record CreateDisciplinaRequest(string Nome);
    public sealed record UpdateDisciplinaRequest(string Nome);

}