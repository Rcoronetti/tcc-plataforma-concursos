namespace TccConcursos.Blazor.Server.Services;

public sealed class ConcursosApi
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ConcursosApi(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }


    public sealed record AuthResult<T>(bool Success, T? Data, string? ErrorMessage)
    {
        public static AuthResult<T> Ok(T data) => new(true, data, null);
        public static AuthResult<T> Fail(string? error) => new(false, default, error);
    }

    public async Task<AuthResult<UsuarioProfileDto>> RegisterUsuarioAsync(RegisterUsuarioRequest request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var resp = await client.PostAsJsonAsync("usuarios/cadastro", request, ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            return AuthResult<UsuarioProfileDto>.Fail(string.IsNullOrWhiteSpace(body) ? "Não foi possível concluir o cadastro." : body.Trim('"'));
        }

        var data = await resp.Content.ReadFromJsonAsync<UsuarioProfileDto>(cancellationToken: ct);
        return data is null
            ? AuthResult<UsuarioProfileDto>.Fail("API não retornou os dados do usuário.")
            : AuthResult<UsuarioProfileDto>.Ok(data);
    }

    public async Task<AuthResult<UsuarioProfileDto>> LoginUsuarioAsync(LoginUsuarioRequest request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var resp = await client.PostAsJsonAsync("usuarios/login", request, ct);

        if (!resp.IsSuccessStatusCode)
        {
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return AuthResult<UsuarioProfileDto>.Fail("Credenciais inválidas. Faça seu cadastro se for o primeiro acesso.");
            }

            var body = await resp.Content.ReadAsStringAsync(ct);
            return AuthResult<UsuarioProfileDto>.Fail(string.IsNullOrWhiteSpace(body) ? "Não foi possível autenticar." : body.Trim('"'));
        }

        var data = await resp.Content.ReadFromJsonAsync<UsuarioProfileDto>(cancellationToken: ct);
        return data is null
            ? AuthResult<UsuarioProfileDto>.Fail("API não retornou os dados do usuário.")
            : AuthResult<UsuarioProfileDto>.Ok(data);
    }

    public async Task<UsuarioProfileDto?> GetUsuarioProfileAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        return await client.GetFromJsonAsync<UsuarioProfileDto>($"usuarios/{usuarioId}/perfil", ct);
    }

    public async Task<AuthResult<UsuarioProfileDto>> UpdateUsuarioProfileAsync(Guid usuarioId, UpdateUsuarioProfileRequest request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var resp = await client.PutAsJsonAsync($"usuarios/{usuarioId}/perfil", request, ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            return AuthResult<UsuarioProfileDto>.Fail(string.IsNullOrWhiteSpace(body) ? "Não foi possível atualizar o perfil." : body.Trim('"'));
        }

        var data = await resp.Content.ReadFromJsonAsync<UsuarioProfileDto>(cancellationToken: ct);
        return data is null
            ? AuthResult<UsuarioProfileDto>.Fail("API não retornou o perfil atualizado.")
            : AuthResult<UsuarioProfileDto>.Ok(data);
    }

    public async Task<AuthResult<bool>> ChangeUsuarioPasswordAsync(Guid usuarioId, ChangeUsuarioPasswordRequest request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var resp = await client.PutAsJsonAsync($"usuarios/{usuarioId}/senha", request, ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            return AuthResult<bool>.Fail(string.IsNullOrWhiteSpace(body) ? "Não foi possível alterar a senha." : body.Trim('"'));
        }

        return AuthResult<bool>.Ok(true);
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

    public async Task<List<TopicoDto>> GetTopicosAsync(Guid disciplinaId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var data = await client.GetFromJsonAsync<List<TopicoDto>>($"disciplinas/{disciplinaId}/topicos", ct);
        return data ?? new List<TopicoDto>();
    }

    public async Task<TopicoDto> CreateTopicoAsync(Guid disciplinaId, CreateTopicoRequest request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");

        var resp = await client.PostAsJsonAsync($"disciplinas/{disciplinaId}/topicos", request, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Erro ao criar tópico. Status={(int)resp.StatusCode}. Body={body}");
        }

        var created = await resp.Content.ReadFromJsonAsync<TopicoDto>(cancellationToken: ct);
        return created ?? throw new InvalidOperationException("API não retornou o tópico criado.");
    }

    public async Task<TopicoDto?> UpdateTopicoAsync(Guid disciplinaId, Guid topicoId, UpdateTopicoRequest request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");

        var resp = await client.PutAsJsonAsync($"disciplinas/{disciplinaId}/topicos/{topicoId}", request, ct);
        if (!resp.IsSuccessStatusCode) return null;

        return await resp.Content.ReadFromJsonAsync<TopicoDto>(cancellationToken: ct);
    }

    public async Task<bool> DeleteTopicoAsync(Guid disciplinaId, Guid topicoId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var resp = await client.DeleteAsync($"disciplinas/{disciplinaId}/topicos/{topicoId}", ct);
        return resp.IsSuccessStatusCode;
    }

    public async Task<DisciplinaDto?> GetDisciplinaAsync(Guid concursoId, Guid disciplinaId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        return await client.GetFromJsonAsync<DisciplinaDto>($"concursos/{concursoId}/disciplinas/{disciplinaId}", ct);
    }

    public async Task<ConcursoDto?> GetConcursoAsync(Guid id, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        return await client.GetFromJsonAsync<ConcursoDto>($"concursos/{id}", ct);
    }

    public async Task<TopicoDto?> GetTopicoAsync(Guid disciplinaId, Guid topicoId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        return await client.GetFromJsonAsync<TopicoDto>($"disciplinas/{disciplinaId}/topicos/{topicoId}", ct);
    }

    public sealed record SessaoEstudoDto(
    Guid Id,
    Guid TopicoId,
    DateTime Inicio,
    DateTime Fim,
    int Tipo,
    int? QuestoesTotal,
    int? QuestoesAcertos
);

    public sealed record CreateSessaoEstudoRequest(
        DateTime Inicio,
        DateTime Fim,
        int Tipo,
        int? QuestoesTotal,
        int? QuestoesAcertos
    );

    public sealed record UpdateSessaoEstudoRequest(
        DateTime Inicio,
        DateTime Fim,
        int Tipo,
        int? QuestoesTotal,
        int? QuestoesAcertos
    );

    public async Task<List<SessaoEstudoDto>> GetSessoesAsync(Guid topicoId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var data = await client.GetFromJsonAsync<List<SessaoEstudoDto>>($"topicos/{topicoId}/sessoes", ct);
        return data ?? new List<SessaoEstudoDto>();
    }

    public async Task<SessaoEstudoDto> CreateSessaoAsync(Guid topicoId, CreateSessaoEstudoRequest request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");

        var resp = await client.PostAsJsonAsync($"topicos/{topicoId}/sessoes", request, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Erro ao criar sessão. Status={(int)resp.StatusCode}. Body={body}");
        }

        var created = await resp.Content.ReadFromJsonAsync<SessaoEstudoDto>(cancellationToken: ct);
        return created ?? throw new InvalidOperationException("API não retornou a sessão criada.");
    }

    public async Task<SessaoEstudoDto?> UpdateSessaoAsync(Guid topicoId, Guid sessaoId, UpdateSessaoEstudoRequest request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");

        Console.WriteLine($"UPDATE SESSAO: Tipo={request.Tipo}, Total={request.QuestoesTotal}, Acertos={request.QuestoesAcertos}");

        var resp = await client.PutAsJsonAsync($"topicos/{topicoId}/sessoes/{sessaoId}", request, ct);
        if (!resp.IsSuccessStatusCode) return null;

        return await resp.Content.ReadFromJsonAsync<SessaoEstudoDto>(cancellationToken: ct);
    }

    public async Task<bool> DeleteSessaoAsync(Guid topicoId, Guid sessaoId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var resp = await client.DeleteAsync($"topicos/{topicoId}/sessoes/{sessaoId}", ct);
        return resp.IsSuccessStatusCode;
    }


    public sealed record RegisterUsuarioRequest(string Nome, string Cpf, string Email, string Senha);
    public sealed record LoginUsuarioRequest(string Login, string Senha);
    public sealed record UpdateUsuarioProfileRequest(string Nome, string Endereco, string Telefone, string Bio, string FotoUrl);
    public sealed record ChangeUsuarioPasswordRequest(string SenhaAtual, string NovaSenha);
    public sealed record UsuarioProfileDto(Guid Id, string Nome, string Email, string Cpf, string Endereco, string Telefone, string Bio, string FotoUrl);

    public sealed record ConcursoDto(Guid Id, string Nome, DateOnly? DataProva);
    public sealed record CreateConcursoRequest(string Nome, DateOnly? DataProva);
    public sealed record UpdateConcursoRequest(string Nome, DateOnly? DataProva);
    public sealed record DisciplinaDto(Guid Id, Guid ConcursoId, string Nome);
    public sealed record CreateDisciplinaRequest(string Nome);
    public sealed record UpdateDisciplinaRequest(string Nome);
    public sealed record TopicoDto(Guid Id, Guid DisciplinaId, string Nome);
    public sealed record CreateTopicoRequest(string Nome);
    public sealed record UpdateTopicoRequest(string Nome);

    public async Task<DashboardResumoDto?> GetDashboardResumoAsync(CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        return await client.GetFromJsonAsync<DashboardResumoDto>("dashboard/resumo", ct);
    }

    public async Task<DashboardPorConcursoDto?> GetDashboardPorConcursoAsync(Guid concursoId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        return await client.GetFromJsonAsync<DashboardPorConcursoDto>($"dashboard/por-concurso/{concursoId}", ct);
    }

    public async Task<List<DashboardPorDisciplinaDto>> GetDashboardDisciplinasPorConcursoAsync(Guid concursoId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var data = await client.GetFromJsonAsync<List<DashboardPorDisciplinaDto>>($"dashboard/por-concurso/{concursoId}/disciplinas", ct);
        return data ?? new List<DashboardPorDisciplinaDto>();
    }

    public async Task<List<DashboardPorTopicoDto>> GetDashboardTopicosPorDisciplinaAsync(Guid disciplinaId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var data = await client.GetFromJsonAsync<List<DashboardPorTopicoDto>>($"dashboard/por-disciplina/{disciplinaId}/topicos", ct);
        return data ?? new List<DashboardPorTopicoDto>();
    }

    public sealed record DashboardResumoDto(
        int TotalSessoes,
        int TotalMinutosEstudados,
        int TotalQuestoes,
        int TotalAcertos,
        double? TaxaAcertoPercentual
    );

    public sealed record DashboardPorConcursoDto(
        Guid ConcursoId,
        string ConcursoNome,
        int TotalSessoes,
        int TotalMinutosEstudados,
        int TotalQuestoes,
        int TotalAcertos,
        double? TaxaAcertoPercentual
    );

    public sealed record DashboardPorDisciplinaDto(
        Guid DisciplinaId,
        string DisciplinaNome,
        int TotalSessoes,
        int TotalMinutosEstudados,
        int TotalQuestoes,
        int TotalAcertos,
        double? TaxaAcertoPercentual
    );

    public sealed record DashboardPorTopicoDto(
        Guid TopicoId,
        Guid DisciplinaId,
        string TopicoNome,
        int TotalSessoes,
        int TotalMinutosEstudados,
        int TotalQuestoes,
        int TotalAcertos,
        double? TaxaAcertoPercentual
    );


}