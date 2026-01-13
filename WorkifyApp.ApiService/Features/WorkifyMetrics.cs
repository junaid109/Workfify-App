using System.Diagnostics.Metrics;

namespace WorkifyApp.ApiService.Features;

public class WorkifyMetrics
{
    private readonly Counter<int> _projectsCreated;
    private readonly Counter<double> _revenue;

    public WorkifyMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("WorkifyApp");
        _projectsCreated = meter.CreateCounter<int>("workify.projects.created");
        _revenue = meter.CreateCounter<double>("workify.revenue");
    }

    public void ProjectCreated() => _projectsCreated.Add(1);
    public void RevenueGenerated(double amount) => _revenue.Add(amount);
}
