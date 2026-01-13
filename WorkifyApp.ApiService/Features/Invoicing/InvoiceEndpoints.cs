using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WorkifyApp.ApiService.Data;
using WorkifyApp.ApiService.Domain;
using WorkifyApp.ApiService.Features;

namespace WorkifyApp.ApiService.Features.Invoicing;

public static class InvoiceEndpoints
{
    public static void MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices")
            .WithTags("Invoices")
            .WithOpenApi();

        group.MapGet("/", GetInvoicesAsync);
        group.MapGet("/{id}", GetInvoiceByIdAsync);
        group.MapPost("/", CreateInvoiceAsync);
        group.MapPost("/{id}/generate-pdf", GeneratePdfAsync);
    }

    static async Task<Ok<List<Invoice>>> GetInvoicesAsync(WorkifyDbContext db)
    {
        return TypedResults.Ok(await db.Invoices.Include(i => i.Items).Include(i => i.Client).ToListAsync());
    }

    static async Task<Results<Ok<Invoice>, NotFound>> GetInvoiceByIdAsync(Guid id, WorkifyDbContext db)
    {
        var invoice = await db.Invoices
            .Include(i => i.Items)
            .Include(i => i.Client)
            .FirstOrDefaultAsync(i => i.Id == id);
            
        return invoice is not null ? TypedResults.Ok(invoice) : TypedResults.NotFound();
    }

    static async Task<Created<Invoice>> CreateInvoiceAsync(Invoice invoice, WorkifyDbContext db, WorkifyMetrics metrics)
    {
        // Simple recalculation of total
        if (invoice.Items != null)
        {
            invoice.TotalAmount = invoice.Items.Sum(i => i.Quantity * i.UnitPrice);
        }

        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        metrics.RevenueGenerated((double)invoice.TotalAmount);

        return TypedResults.Created($"/api/invoices/{invoice.Id}", invoice);
    }

    static async Task<Results<Accepted, NotFound>> GeneratePdfAsync(Guid id, WorkifyDbContext db)
    {
        // Here we would enqueue a job for the background worker
        // For now, we just acknowledge receipt
        var invoice = await db.Invoices.FindAsync(id);
        if (invoice is null) return TypedResults.NotFound();

        // TODO: Enqueue background job (e.g. via Channel or Queue)
        
        return TypedResults.Accepted($"/api/invoices/{id}");
    }
}
