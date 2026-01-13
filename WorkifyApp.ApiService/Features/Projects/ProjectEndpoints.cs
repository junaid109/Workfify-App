using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WorkifyApp.ApiService.Data;
using WorkifyApp.ApiService.Domain;
using Microsoft.Extensions.Caching.Hybrid;

namespace WorkifyApp.ApiService.Features.Projects;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects")
            .WithTags("Projects")
            .WithOpenApi();

        group.MapGet("/", GetProjectsAsync);
        group.MapGet("/{id}", GetProjectByIdAsync);
        group.MapPost("/", CreateProjectAsync);
    }

    static async Task<Ok<List<Project>>> GetProjectsAsync(WorkifyDbContext db)
    {
        return TypedResults.Ok(await db.Projects.Include(p => p.Client).ToListAsync());
    }



    static async Task<Results<Ok<Project>, NotFound>> GetProjectByIdAsync(Guid id, WorkifyDbContext db, HybridCache cache, CancellationToken ct)
    {
        var project = await cache.GetOrCreateAsync(
            $"project-{id}",
            async token => await db.Projects.Include(p => p.Client).FirstOrDefaultAsync(p => p.Id == id, token),
            cancellationToken: ct
        );

        return project is not null ? TypedResults.Ok(project) : TypedResults.NotFound();
    }

    static async Task<Created<Project>> CreateProjectAsync(Project project, WorkifyDbContext db)
    {
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return TypedResults.Created($"/api/projects/{project.Id}", project);
    }
}
