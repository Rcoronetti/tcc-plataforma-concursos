using Microsoft.EntityFrameworkCore;
using TccConcursos.Api.Contracts.Concursos;
using TccConcursos.Api.Contracts.Dashboard;
using TccConcursos.Api.Contracts.Disciplinas;
using TccConcursos.Api.Contracts.SessoesEstudo;
using TccConcursos.Api.Contracts.Topicos;
using TccConcursos.Domain.Entities;
using TccConcursos.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins("https://localhost:5000", "https://localhost:5001", "http://localhost:5000", "http://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowBlazor");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

# region Consursos
var concursos = app.MapGroup("/concursos").WithTags("Concursos");

concursos.MapPost("/", async (CreateConcursoRequest request, ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Nome))
        return Results.BadRequest("Nome é obrigatório.");

    if (request.Nome.Length > 200)
        return Results.BadRequest("Nome deve ter no máximo 200 caracteres.");

    var entity = new Concurso
    {
        Nome = request.Nome.Trim(),
        DataProva = request.DataProva
    };

    db.Concursos.Add(entity);
    await db.SaveChangesAsync();

    var response = new ConcursoResponse(entity.Id, entity.Nome, entity.DataProva);
    return Results.Created($"/concursos/{entity.Id}", response);
})
.WithOpenApi();

concursos.MapGet("/", async (ApplicationDbContext db) =>
{
    var list = await db.Concursos
        .AsNoTracking()
        .OrderBy(x => x.Nome)
        .Select(x => new ConcursoResponse(x.Id, x.Nome, x.DataProva))
        .ToListAsync();

    return Results.Ok(list);
})
.WithOpenApi();

concursos.MapGet("/{id:guid}", async (Guid id, ApplicationDbContext db) =>
{
    var c = await db.Concursos
        .AsNoTracking()
        .Where(x => x.Id == id)
        .Select(x => new ConcursoResponse(x.Id, x.Nome, x.DataProva))
        .FirstOrDefaultAsync();

    return c is null ? Results.NotFound() : Results.Ok(c);
})
.WithOpenApi();

concursos.MapPut("/{id:guid}", async (Guid id, UpdateConcursoRequest request, ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Nome))
        return Results.BadRequest("Nome é obrigatório.");

    if (request.Nome.Length > 200)
        return Results.BadRequest("Nome deve ter no máximo 200 caracteres.");

    var entity = await db.Concursos.FirstOrDefaultAsync(x => x.Id == id);
    if (entity is null)
        return Results.NotFound();

    entity.Nome = request.Nome.Trim();
    entity.DataProva = request.DataProva;

    await db.SaveChangesAsync();

    var response = new ConcursoResponse(entity.Id, entity.Nome, entity.DataProva);
    return Results.Ok(response);
})
.WithOpenApi();

concursos.MapDelete("/{id:guid}", async (Guid id, ApplicationDbContext db) =>
{
    var entity = await db.Concursos.FirstOrDefaultAsync(x => x.Id == id);
    if (entity is null)
        return Results.NotFound();

    db.Concursos.Remove(entity);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithOpenApi();
#endregion

#region Disciplinas
var disciplinas = app.MapGroup("/concursos/{concursoId:guid}/disciplinas")
    .WithTags("Disciplinas");

disciplinas.MapPost("/", async (Guid concursoId, CreateDisciplinaRequest request, ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Nome))
        return Results.BadRequest("Nome é obrigatório.");

    if (request.Nome.Length > 200)
        return Results.BadRequest("Nome deve ter no máximo 200 caracteres.");

    var concursoExiste = await db.Concursos.AnyAsync(x => x.Id == concursoId);
    if (!concursoExiste)
        return Results.NotFound("Concurso não encontrado.");

    var jaExiste = await db.Disciplinas
    .AnyAsync(x => x.ConcursoId == concursoId && x.Nome.ToLower() == request.Nome.ToLower());
    if (jaExiste)
        return Results.Conflict("Já existe uma disciplina com este nome neste concurso.");

    var entity = new Disciplina
    {
        ConcursoId = concursoId,
        Nome = request.Nome.Trim()
    };

    db.Disciplinas.Add(entity);
    await db.SaveChangesAsync();

    var response = new DisciplinaResponse(entity.Id, entity.ConcursoId, entity.Nome);
    return Results.Created($"/concursos/{concursoId}/disciplinas/{entity.Id}", response);
})
.WithOpenApi();

disciplinas.MapGet("/", async (Guid concursoId, ApplicationDbContext db) =>
{
    var concursoExiste = await db.Concursos.AnyAsync(x => x.Id == concursoId);
    if (!concursoExiste)
        return Results.NotFound("Concurso não encontrado.");

    var list = await db.Disciplinas
        .AsNoTracking()
        .Where(x => x.ConcursoId == concursoId)
        .OrderBy(x => x.Nome)
        .Select(x => new DisciplinaResponse(x.Id, x.ConcursoId, x.Nome))
        .ToListAsync();

    return Results.Ok(list);
})
.WithOpenApi();

disciplinas.MapGet("/{disciplinaId:guid}", async (Guid concursoId, Guid disciplinaId, ApplicationDbContext db) =>
{
    var d = await db.Disciplinas
        .AsNoTracking()
        .Where(x => x.ConcursoId == concursoId && x.Id == disciplinaId)
        .Select(x => new DisciplinaResponse(x.Id, x.ConcursoId, x.Nome))
        .FirstOrDefaultAsync();

    return d is null ? Results.NotFound() : Results.Ok(d);
})
.WithOpenApi();

disciplinas.MapPut("/{disciplinaId:guid}", async (Guid concursoId, Guid disciplinaId, UpdateDisciplinaRequest request, ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Nome))
        return Results.BadRequest("Nome é obrigatório.");

    if (request.Nome.Length > 200)
        return Results.BadRequest("Nome deve ter no máximo 200 caracteres.");

    var entity = await db.Disciplinas
        .FirstOrDefaultAsync(x => x.ConcursoId == concursoId && x.Id == disciplinaId);

    if (entity is null)
        return Results.NotFound();

    entity.Nome = request.Nome.Trim();
    await db.SaveChangesAsync();

    var response = new DisciplinaResponse(entity.Id, entity.ConcursoId, entity.Nome);
    return Results.Ok(response);
})
.WithOpenApi();

disciplinas.MapDelete("/{disciplinaId:guid}", async (Guid concursoId, Guid disciplinaId, ApplicationDbContext db) =>
{
    var entity = await db.Disciplinas
        .FirstOrDefaultAsync(x => x.ConcursoId == concursoId && x.Id == disciplinaId);

    if (entity is null)
        return Results.NotFound();

    db.Disciplinas.Remove(entity);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithOpenApi();
#endregion

#region Topicos
var topicos = app.MapGroup("/disciplinas/{disciplinaId:guid}/topicos")
    .WithTags("Topicos");

topicos.MapPost("/", async (Guid disciplinaId, CreateTopicoRequest request, ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Nome))
        return Results.BadRequest("Nome é obrigatório.");

    if (request.Nome.Length > 200)
        return Results.BadRequest("Nome deve ter no máximo 200 caracteres.");

    var disciplinaExiste = await db.Disciplinas.AnyAsync(x => x.Id == disciplinaId);
    if (!disciplinaExiste)
        return Results.NotFound("Disciplina não encontrada.");

    var nomeNormalizado = request.Nome.Trim().ToLower();

    var jaExiste = await db.Topicos
        .AnyAsync(x => x.DisciplinaId == disciplinaId && x.Nome.ToLower() == nomeNormalizado);

    if (jaExiste)
        return Results.Conflict("Já existe um tópico com este nome nesta disciplina.");

    var entity = new Topico
    {
        DisciplinaId = disciplinaId,
        Nome = request.Nome.Trim()
    };

    db.Topicos.Add(entity);
    await db.SaveChangesAsync();

    var response = new TopicoResponse(entity.Id, entity.DisciplinaId, entity.Nome);
    return Results.Created($"/disciplinas/{disciplinaId}/topicos/{entity.Id}", response);
})
.WithOpenApi();


topicos.MapGet("/", async (Guid disciplinaId, ApplicationDbContext db) =>
{
    var disciplinaExiste = await db.Disciplinas.AnyAsync(x => x.Id == disciplinaId);
    if (!disciplinaExiste)
        return Results.NotFound("Disciplina não encontrada.");

    var list = await db.Topicos
        .AsNoTracking()
        .Where(x => x.DisciplinaId == disciplinaId)
        .OrderBy(x => x.Nome)
        .Select(x => new TopicoResponse(x.Id, x.DisciplinaId, x.Nome))
        .ToListAsync();

    return Results.Ok(list);
})
.WithOpenApi();

topicos.MapGet("/{topicoId:guid}", async (Guid disciplinaId, Guid topicoId, ApplicationDbContext db) =>
{
    var t = await db.Topicos
        .AsNoTracking()
        .Where(x => x.DisciplinaId == disciplinaId && x.Id == topicoId)
        .Select(x => new TopicoResponse(x.Id, x.DisciplinaId, x.Nome))
        .FirstOrDefaultAsync();

    return t is null ? Results.NotFound() : Results.Ok(t);
})
.WithOpenApi();

topicos.MapPut("/{topicoId:guid}", async (Guid disciplinaId, Guid topicoId, UpdateTopicoRequest request, ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Nome))
        return Results.BadRequest("Nome é obrigatório.");

    if (request.Nome.Length > 200)
        return Results.BadRequest("Nome deve ter no máximo 200 caracteres.");

    var entity = await db.Topicos
        .FirstOrDefaultAsync(x => x.DisciplinaId == disciplinaId && x.Id == topicoId);

    if (entity is null)
        return Results.NotFound();

    var nomeNormalizado = request.Nome.Trim().ToLower();

    var jaExiste = await db.Topicos.AnyAsync(x =>
        x.DisciplinaId == disciplinaId &&
        x.Id != topicoId &&
        x.Nome.ToLower() == nomeNormalizado);

    if (jaExiste)
        return Results.Conflict("Já existe um tópico com este nome nesta disciplina.");

    entity.Nome = request.Nome.Trim();
    await db.SaveChangesAsync();

    var response = new TopicoResponse(entity.Id, entity.DisciplinaId, entity.Nome);
    return Results.Ok(response);
})
.WithOpenApi();

topicos.MapDelete("/{topicoId:guid}", async (Guid disciplinaId, Guid topicoId, ApplicationDbContext db) =>
{
    var entity = await db.Topicos
        .FirstOrDefaultAsync(x => x.DisciplinaId == disciplinaId && x.Id == topicoId);

    if (entity is null)
        return Results.NotFound();

    db.Topicos.Remove(entity);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithOpenApi();
#endregion

#region Sessões de Estudo
var sessoes = app.MapGroup("/topicos/{topicoId:guid}/sessoes")
    .WithTags("SessoesEstudo");

sessoes.MapPost("/", async (Guid topicoId, CreateSessaoEstudoRequest request, ApplicationDbContext db) =>
{
    if (request.Fim <= request.Inicio)
        return Results.BadRequest("Fim deve ser maior que Início.");

    var duracao = request.Fim - request.Inicio;
    if (duracao.TotalHours > 24)
        return Results.BadRequest("Duração da sessão não pode exceder 24 horas.");

    var topicoExiste = await db.Topicos.AnyAsync(x => x.Id == topicoId);
    if (!topicoExiste)
        return Results.NotFound("Tópico não encontrado.");

    // Tipo: 1=Teoria, 2=Revisao, 3=Questoes
    if (request.Tipo is < 1 or > 3)
        return Results.BadRequest("Tipo inválido. Use 1=Teoria, 2=Revisão, 3=Questões.");

    if (request.Tipo == 3)
    {
        if (request.QuestoesTotal is null or <= 0)
            return Results.BadRequest("QuestoesTotal é obrigatório e deve ser > 0 quando Tipo=Questões.");

        if (request.QuestoesAcertos is null or < 0)
            return Results.BadRequest("QuestoesAcertos é obrigatório e deve ser >= 0 quando Tipo=Questões.");

        if (request.QuestoesAcertos > request.QuestoesTotal)
            return Results.BadRequest("QuestoesAcertos não pode ser maior que QuestoesTotal.");
    }
    else
    {
        if (request.QuestoesTotal is not null && request.QuestoesTotal <= 0)
            return Results.BadRequest("QuestoesTotal, se informado, deve ser > 0.");

        if (request.QuestoesAcertos is not null && request.QuestoesAcertos < 0)
            return Results.BadRequest("QuestoesAcertos, se informado, deve ser >= 0.");

        if (request.QuestoesTotal is not null && request.QuestoesAcertos is not null &&
            request.QuestoesAcertos > request.QuestoesTotal)
            return Results.BadRequest("QuestoesAcertos não pode ser maior que QuestoesTotal.");
    }

    var entity = new SessaoEstudo
    {
        TopicoId = topicoId,
        Inicio = request.Inicio.ToUniversalTime(),
        Fim = request.Fim.ToUniversalTime(),
        Tipo = (TccConcursos.Domain.Enums.TipoSessaoEstudo)request.Tipo,
        QuestoesTotal = request.QuestoesTotal,
        QuestoesAcertos = request.QuestoesAcertos
    };

    db.SessoesEstudo.Add(entity);
    await db.SaveChangesAsync();

    var response = new SessaoEstudoResponse(
        entity.Id, entity.TopicoId, entity.Inicio, entity.Fim,
        (int)entity.Tipo, entity.QuestoesTotal, entity.QuestoesAcertos);

    return Results.Created($"/topicos/{topicoId}/sessoes/{entity.Id}", response);
})
.WithOpenApi();

sessoes.MapGet("/", async (Guid topicoId, ApplicationDbContext db) =>
{
    var topicoExiste = await db.Topicos.AnyAsync(x => x.Id == topicoId);
    if (!topicoExiste)
        return Results.NotFound("Tópico não encontrado.");

    var list = await db.SessoesEstudo
        .AsNoTracking()
        .Where(x => x.TopicoId == topicoId)
        .OrderByDescending(x => x.Inicio)
        .Select(x => new SessaoEstudoResponse(
            x.Id, x.TopicoId, x.Inicio, x.Fim,
            (int)x.Tipo, x.QuestoesTotal, x.QuestoesAcertos))
        .ToListAsync();

    return Results.Ok(list);
})
.WithOpenApi();

sessoes.MapGet("/{sessaoId:guid}", async (Guid topicoId, Guid sessaoId, ApplicationDbContext db) =>
{
    var s = await db.SessoesEstudo
        .AsNoTracking()
        .Where(x => x.TopicoId == topicoId && x.Id == sessaoId)
        .Select(x => new SessaoEstudoResponse(
            x.Id, x.TopicoId, x.Inicio, x.Fim,
            (int)x.Tipo, x.QuestoesTotal, x.QuestoesAcertos))
        .FirstOrDefaultAsync();

    return s is null ? Results.NotFound() : Results.Ok(s);
})
.WithOpenApi();

sessoes.MapPut("/{sessaoId:guid}", async (Guid topicoId, Guid sessaoId, UpdateSessaoEstudoRequest request, ApplicationDbContext db) =>
{
    if (request.Fim <= request.Inicio)
        return Results.BadRequest("Fim deve ser maior que Início.");

    var duracao = request.Fim - request.Inicio;
    if (duracao.TotalHours > 24)
        return Results.BadRequest("Duração da sessão não pode exceder 24 horas.");

    if (request.Tipo is < 1 or > 3)
        return Results.BadRequest("Tipo inválido. Use 1=Teoria, 2=Revisão, 3=Questões.");

    if (request.Tipo == 3)
    {
        if (request.QuestoesTotal is null or <= 0)
            return Results.BadRequest("QuestoesTotal é obrigatório e deve ser > 0 quando Tipo=Questões.");

        if (request.QuestoesAcertos is null or < 0)
            return Results.BadRequest("QuestoesAcertos é obrigatório e deve ser >= 0 quando Tipo=Questões.");

        if (request.QuestoesAcertos > request.QuestoesTotal)
            return Results.BadRequest("QuestoesAcertos não pode ser maior que QuestoesTotal.");
    }
    else
    {
        if (request.QuestoesTotal is not null && request.QuestoesTotal <= 0)
            return Results.BadRequest("QuestoesTotal, se informado, deve ser > 0.");

        if (request.QuestoesAcertos is not null && request.QuestoesAcertos < 0)
            return Results.BadRequest("QuestoesAcertos, se informado, deve ser >= 0.");

        if (request.QuestoesTotal is not null && request.QuestoesAcertos is not null &&
            request.QuestoesAcertos > request.QuestoesTotal)
            return Results.BadRequest("QuestoesAcertos não pode ser maior que QuestoesTotal.");
    }

    var entity = await db.SessoesEstudo
        .FirstOrDefaultAsync(x => x.TopicoId == topicoId && x.Id == sessaoId);

    if (entity is null)
        return Results.NotFound();

    entity.Inicio = request.Inicio.ToUniversalTime();
    entity.Fim = request.Fim.ToUniversalTime();
    entity.Tipo = (TccConcursos.Domain.Enums.TipoSessaoEstudo)request.Tipo;
    entity.QuestoesTotal = request.QuestoesTotal;
    entity.QuestoesAcertos = request.QuestoesAcertos;

    await db.SaveChangesAsync();

    var response = new SessaoEstudoResponse(
        entity.Id, entity.TopicoId, entity.Inicio, entity.Fim,
        (int)entity.Tipo, entity.QuestoesTotal, entity.QuestoesAcertos);

    return Results.Ok(response);
})
.WithOpenApi();

sessoes.MapDelete("/{sessaoId:guid}", async (Guid topicoId, Guid sessaoId, ApplicationDbContext db) =>
{
    var entity = await db.SessoesEstudo
        .FirstOrDefaultAsync(x => x.TopicoId == topicoId && x.Id == sessaoId);

    if (entity is null)
        return Results.NotFound();

    db.SessoesEstudo.Remove(entity);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithOpenApi();
#endregion

#region Dashboard
var dashboard = app.MapGroup("/dashboard")
    .WithTags("Dashboard");

#region Resumo
dashboard.MapGet("/resumo", async (DateOnly? dataInicio, DateOnly? dataFim, ApplicationDbContext db) =>
{
    DateTime? inicio = dataInicio?.ToDateTime(TimeOnly.MinValue);
    DateTime? fim = dataFim?.ToDateTime(TimeOnly.MaxValue);

    var query = db.SessoesEstudo.AsNoTracking().AsQueryable();

    if (inicio.HasValue)
        query = query.Where(x => x.Inicio >= inicio.Value);

    if (fim.HasValue)
        query = query.Where(x => x.Inicio <= fim.Value);

    var totalSessoes = await query.CountAsync();

    var totalMinutosDouble = await query
        .Select(x => (x.Fim - x.Inicio).TotalSeconds / 60.0)
        .SumAsync();

    var totalMinutos = (int)Math.Round(totalMinutosDouble, 0);

    var totalQuestoes = await query
        .Where(x => x.QuestoesTotal != null)
        .Select(x => x.QuestoesTotal!.Value)
        .SumAsync();

    var totalAcertos = await query
        .Where(x => x.QuestoesAcertos != null)
        .Select(x => x.QuestoesAcertos!.Value)
        .SumAsync();

    double? taxa = totalQuestoes > 0
        ? Math.Round((double)totalAcertos / totalQuestoes * 100.0, 2)
        : null;

    return Results.Ok(new DashboardResumoResponse(
        totalSessoes,
        totalMinutos,
        totalQuestoes,
        totalAcertos,
        taxa
    ));
})
.WithOpenApi();
#endregion

#region PorDisciplina
dashboard.MapGet("/por-disciplina", async (DateOnly? dataInicio, DateOnly? dataFim, ApplicationDbContext db) =>
{
    DateTime? inicio = dataInicio?.ToDateTime(TimeOnly.MinValue);
    DateTime? fim = dataFim?.ToDateTime(TimeOnly.MaxValue);

    var sessoesQuery = db.SessoesEstudo.AsNoTracking().AsQueryable();

    if (inicio.HasValue)
        sessoesQuery = sessoesQuery.Where(x => x.Inicio >= inicio.Value);

    if (fim.HasValue)
        sessoesQuery = sessoesQuery.Where(x => x.Inicio <= fim.Value);
    var query =
        from s in sessoesQuery
        join t in db.Topicos.AsNoTracking() on s.TopicoId equals t.Id
        join d in db.Disciplinas.AsNoTracking() on t.DisciplinaId equals d.Id
        group s by new { d.Id, d.Nome } into g
        orderby g.Key.Nome
        select new
        {
            DisciplinaId = g.Key.Id,
            DisciplinaNome = g.Key.Nome,
            TotalSessoes = g.Count(),
            TotalMinutos = g.Sum(x => (x.Fim - x.Inicio).TotalSeconds / 60.0),
            TotalQuestoes = g.Sum(x => x.QuestoesTotal ?? 0),
            TotalAcertos = g.Sum(x => x.QuestoesAcertos ?? 0)
        };

    var data = await query.ToListAsync();

    var response = data.Select(x =>
    {
        double? taxa = x.TotalQuestoes > 0
            ? Math.Round((double)x.TotalAcertos / x.TotalQuestoes * 100.0, 2)
            : null;

        return new DashboardPorDisciplinaResponse(
            x.DisciplinaId,
            x.DisciplinaNome,
            x.TotalSessoes,
            (int)Math.Round(x.TotalMinutos, 0),
            x.TotalQuestoes,
            x.TotalAcertos,
            taxa
        );
    });

    return Results.Ok(response);
})
.WithOpenApi();
#endregion

#region PorConcurso
dashboard.MapGet("/por-concurso", async (DateOnly? dataInicio, DateOnly? dataFim, ApplicationDbContext db) =>
{
    DateTime? inicio = dataInicio?.ToDateTime(TimeOnly.MinValue);
    DateTime? fim = dataFim?.ToDateTime(TimeOnly.MaxValue);

    var concursos = await db.Concursos
        .AsNoTracking()
        .OrderBy(x => x.Nome)
        .Select(x => new { x.Id, x.Nome })
        .ToListAsync();

    var sessoesQuery = db.SessoesEstudo.AsNoTracking().AsQueryable();

    if (inicio.HasValue)
        sessoesQuery = sessoesQuery.Where(x => x.Inicio >= inicio.Value);

    if (fim.HasValue)
        sessoesQuery = sessoesQuery.Where(x => x.Inicio <= fim.Value);

    var agregados = await (
        from s in sessoesQuery
        join t in db.Topicos.AsNoTracking() on s.TopicoId equals t.Id
        join d in db.Disciplinas.AsNoTracking() on t.DisciplinaId equals d.Id
        join c in db.Concursos.AsNoTracking() on d.ConcursoId equals c.Id
        group s by new { c.Id, c.Nome } into g
        select new
        {
            ConcursoId = g.Key.Id,
            TotalSessoes = g.Count(),
            TotalMinutos = g.Sum(x => (x.Fim - x.Inicio).TotalSeconds / 60.0),
            TotalQuestoes = g.Sum(x => x.QuestoesTotal ?? 0),
            TotalAcertos = g.Sum(x => x.QuestoesAcertos ?? 0)
        }
    ).ToListAsync();

    var dict = agregados.ToDictionary(x => x.ConcursoId);

    var response = concursos.Select(c =>
    {
        if (!dict.TryGetValue(c.Id, out var a))
        {
            return new DashboardPorConcursoResponse(
                c.Id,
                c.Nome,
                0,
                0,
                0,
                0,
                null
            );
        }

        var totalMinutos = (int)Math.Round(a.TotalMinutos, 0);
        double? taxa = a.TotalQuestoes > 0
            ? Math.Round((double)a.TotalAcertos / a.TotalQuestoes * 100.0, 2)
            : null;

        return new DashboardPorConcursoResponse(
            c.Id,
            c.Nome,
            a.TotalSessoes,
            totalMinutos,
            a.TotalQuestoes,
            a.TotalAcertos,
            taxa
        );
    });

    return Results.Ok(response);
})
.WithOpenApi();
#endregion

#region Por Topico
dashboard.MapGet("/por-topico", async (Guid disciplinaId, DateOnly? dataInicio, DateOnly? dataFim, ApplicationDbContext db) =>
{
    // valida disciplina
    var disciplinaExiste = await db.Disciplinas.AnyAsync(x => x.Id == disciplinaId);
    if (!disciplinaExiste)
        return Results.NotFound("Disciplina não encontrada.");

    DateTime? inicio = dataInicio?.ToDateTime(TimeOnly.MinValue);
    DateTime? fim = dataFim?.ToDateTime(TimeOnly.MaxValue);

    // Base: todos os tópicos da disciplina (para incluir zeros)
    var topicos = await db.Topicos
        .AsNoTracking()
        .Where(x => x.DisciplinaId == disciplinaId)
        .OrderBy(x => x.Nome)
        .Select(x => new { x.Id, x.DisciplinaId, x.Nome })
        .ToListAsync();

    // Sessões filtradas por período
    var sessoesQuery = db.SessoesEstudo.AsNoTracking().AsQueryable();

    if (inicio.HasValue)
        sessoesQuery = sessoesQuery.Where(x => x.Inicio >= inicio.Value);

    if (fim.HasValue)
        sessoesQuery = sessoesQuery.Where(x => x.Inicio <= fim.Value);

    // Agrega sessões por tópico (somente tópicos com sessão)
    var agregados = await (
        from s in sessoesQuery
        join t in db.Topicos.AsNoTracking() on s.TopicoId equals t.Id
        where t.DisciplinaId == disciplinaId
        group s by new { t.Id } into g
        select new
        {
            TopicoId = g.Key.Id,
            TotalSessoes = g.Count(),
            TotalMinutos = g.Sum(x => (x.Fim - x.Inicio).TotalSeconds / 60.0),
            TotalQuestoes = g.Sum(x => x.QuestoesTotal ?? 0),
            TotalAcertos = g.Sum(x => x.QuestoesAcertos ?? 0)
        }
    ).ToListAsync();

    var dict = agregados.ToDictionary(x => x.TopicoId);

    var response = topicos.Select(t =>
    {
        if (!dict.TryGetValue(t.Id, out var a))
        {
            return new DashboardPorTopicoResponse(
                t.Id,
                t.DisciplinaId,
                t.Nome,
                0,
                0,
                0,
                0,
                null
            );
        }

        var totalMinutos = (int)Math.Round(a.TotalMinutos, 0);

        double? taxa = a.TotalQuestoes > 0
            ? Math.Round((double)a.TotalAcertos / a.TotalQuestoes * 100.0, 2)
            : null;

        return new DashboardPorTopicoResponse(
            t.Id,
            t.DisciplinaId,
            t.Nome,
            a.TotalSessoes,
            totalMinutos,
            a.TotalQuestoes,
            a.TotalAcertos,
            taxa
        );
    });

    return Results.Ok(response);
})
.WithOpenApi();
#endregion
#endregion


app.Run();