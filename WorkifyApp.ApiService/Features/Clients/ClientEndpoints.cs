using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WorkifyApp.ApiService.Data;
using WorkifyApp.ApiService.Domain;

namespace WorkifyApp.ApiService.Features.Clients;

public static class ClientEndpoints
{
    public static void MapClientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clients")
            .WithTags("Clients")
            .WithOpenApi();

        group.MapGet("/", GetClientsAsync);
        group.MapGet("/{id}", GetClientByIdAsync);
        group.MapPost("/", CreateClientAsync);
        group.MapPut("/{id}", UpdateClientAsync);
    }

    static async Task<Ok<List<Client>>> GetClientsAsync(WorkifyDbContext db)
    {
        return TypedResults.Ok(await db.Clients.ToListAsync());
    }

    static async Task<Results<Ok<Client>, NotFound>> GetClientByIdAsync(Guid id, WorkifyDbContext db)
    {
        var client = await db.Clients.FindAsync(id);
        return client is not null ? TypedResults.Ok(client) : TypedResults.NotFound();
    }

    static async Task<Created<Client>> CreateClientAsync(Client client, WorkifyDbContext db)
    {
        db.Clients.Add(client);
        await db.SaveChangesAsync();
        return TypedResults.Created($"/api/clients/{client.Id}", client);
    }

    static async Task<Results<NoContent, NotFound>> UpdateClientAsync(Guid id, Client inputClient, WorkifyDbContext db)
    {
        var client = await db.Clients.FindAsync(id);
        if (client is null) return TypedResults.NotFound();

        client.Name = inputClient.Name;
        client.Email = inputClient.Email;
        client.Phone = inputClient.Phone;
        client.CompanyName = inputClient.CompanyName;
        // Update other fields as necessary

        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}
