using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using WorkifyApp.ApiService.Data;

namespace WorkifyApp.ApiService.Features.AI;

public class WorkifyDataPlugin(WorkifyDbContext db)
{
    [KernelFunction, Description("Gets a list of projects that are over the specified budget.")]
    public async Task<string> GetOverBudgetProjectsAsync(
        [Description("The budget limit to check against")] decimal budgetLimit)
    {
        var projects = await db.Projects
            .Where(p => p.Budget > budgetLimit)
            .Select(p => $"{p.Name} (Budget: {p.Budget:C})")
            .ToListAsync();

        return projects.Any() 
            ? string.Join(", ", projects) 
            : "No projects found over the specified budget.";
    }

    [KernelFunction, Description("Gets payment/invoice history for a client by name.")]
    public async Task<string> GetClientPaymentHistoryAsync(
        [Description("The name of the client")] string clientName)
    {
        var client = await db.Clients
            .Include(c => c.Projects)
            .FirstOrDefaultAsync(c => c.Name.Contains(clientName));

        if (client == null) return "Client not found.";

        // Assuming later we link Invoices, for now checking Projects budget as proxy or if we had Invoices
        // Let's check Invoices if we added them to DbContext (we did)
        var invoices = await db.Invoices
            .Where(i => i.ClientId == client.Id)
            .ToListAsync();

        if (!invoices.Any()) return $"No invoices found for {client.Name}.";

        var history = invoices.Select(i => $"Invoice #{i.Id.ToString()[..4]} - {i.TotalAmount:C} ({i.Status}) - Due: {i.DueDate:d}");
        return string.Join("; ", history);
    }
}
