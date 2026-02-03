using Microsoft.EntityFrameworkCore;
using TccConcursos.Api.Contracts.Concursos;
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

app.Run();