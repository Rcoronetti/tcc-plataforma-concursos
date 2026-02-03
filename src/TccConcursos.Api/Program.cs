using Microsoft.EntityFrameworkCore;
using TccConcursos.Api.Contracts.Concursos;
using TccConcursos.Api.Contracts.Disciplinas;
using TccConcursos.Api.Contracts.Topicos;
using TccConcursos.Domain.Entities;
using TccConcursos.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

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

app.Run();