using ContatosGav.Auth;
using ContatosGAV.Models;
using ContatosGAV.Repository;
using ContatosGAV.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Adicionando Swagger para documentação da API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adicionando o DbContext do Entity Framework para Contatos
builder.Services.AddDbContext<ContatoDbContext>();

// Adicionando a Autenticação e Autorização
builder.Services.AddServiceSdk(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Habilitar Swagger em ambiente de desenvolvimento
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoint para listar todos os contatos
app.MapGet("/contato", async (
    ContatoDbContext context,
    ClaimsPrincipal user) =>
{
    try
    {
        var contatos = await context.Contatos.ToListAsync();
        return Results.Ok(contatos);
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Erro interno: {ex.Message}");
    }
})
    .ProducesValidationProblem()
    .RequireAuthorization()
    .Produces(StatusCodes.Status401Unauthorized)
    .WithName("GetContato")
    .WithTags("Contato");

// Endpoint para obter um contato por ID
app.MapGet("/contato/{id}", async (
    Guid id,
    ContatoDbContext context) =>
{
    try
    {
        // Usando LINQ to Entities para encontrar o objeto.
        var contato = await context.Contatos.FindAsync(id);

        if (contato != null)
            return Results.Ok(contato);
        else
            return Results.NotFound("Não foi possível encontrar um contato com esse ID.");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Erro interno: {ex.Message}");
    }
})
    .ProducesValidationProblem()
    .RequireAuthorization()
    .Produces(StatusCodes.Status401Unauthorized)
    .Produces<Contato>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetContatoPorId")
    .WithTags("Contato");

// Endpoint para criar um novo contato
app.MapPost("/contato", (
    ContatoDbContext context,
    ContatoViewModel viewModel) =>
{
    try
    {
        //Validando o objeto recebido com o ViewModel.
        var contato = viewModel.Mapto();
        if (!viewModel.IsValid)
            return Results.BadRequest(viewModel.Notifications);
        context.Contatos.Add(contato);
        context.SaveChanges();
        return Results.Created($"/contato/{contato.Id}", contato);
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Erro interno: {ex.Message}");
    }
})
    .RequireAuthorization()
    .Produces(StatusCodes.Status401Unauthorized).ProducesValidationProblem()
    .Produces<Contato>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("PostContato")
    .WithTags("Contato");

// Endpoint para atualizar um contato existente por ID
app.MapPut("/contato/{id}", async (
        Guid id,
        ContatoDbContext context,
        ContatoViewModel viewModel) =>
{
    try
    {
        // Usando LINQ para encontrar o objeto.
        var contatoExistente = await context.Contatos.AsNoTracking<Contato>()
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (contatoExistente == null)
            return Results.NotFound("Não foi possível encontrar um contato com esse ID.");

        // Validando o objeto recebido com o ViewModel.
        var contato = viewModel.Mapto();
        if (!viewModel.IsValid)
            return Results.BadRequest(viewModel.Notifications);

        contato.Id = contatoExistente.Id;
        context.Contatos.Update(contato);
        var result = await context.SaveChangesAsync();

        return result > 0
            ? Results.NoContent()
            : Results.BadRequest("Ocorreu um problema ao salvar o contato");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Erro interno: {ex.Message}");
    }
})
    .RequireAuthorization()
    .ProducesValidationProblem()
    .Produces(StatusCodes.Status401Unauthorized)
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("PutContato")
    .WithTags("Contato");

// Endpoint para excluir um contato por ID
app.MapDelete("/contato/{id}", async (
    Guid id,
    ContatoDbContext context,
    ClaimsPrincipal user) =>
{
    try
    {
        if (!user.IsInRole("ADM"))
            return Results.Problem("Usuário não autorizado a excluir contatos.");

        //Usando LINQ to Entities para encontrar o objeto.
        var contatoExistente = await context.Contatos.FindAsync(id);

        //Validando se contato existe.
        if (contatoExistente != null)
        {
            context.Contatos.Remove(contatoExistente);
            await context.SaveChangesAsync();
            return Results.Ok("Contato excluído com sucesso");
        }
        else
            return Results.NotFound("Não foi possível encontrar um contato com esse ID.");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Erro interno: {ex.Message}");
    }
})
    //.RequireAuthorization("ADM")
    .ProducesValidationProblem()
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status401Unauthorized)
    .WithName("DeleteContato")
    .WithTags("Contato");

app.Run();
