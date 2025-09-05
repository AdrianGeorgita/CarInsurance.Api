using CarInsurance.Api.Data;
using Microsoft.EntityFrameworkCore;
using CarInsurance.Api.Models;

namespace CarInsurance.Api.Services;

public class PolicyExpirationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PolicyExpirationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
    public PolicyExpirationService(IServiceScopeFactory scopeFactory, ILogger<PolicyExpirationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckExpirationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking policy expirations");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckExpirationsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();


        var today = DateOnly.FromDateTime(DateTime.Now);
        var yesterday = today.AddDays(-1);

        var alreadyLogged = await db.PolicyExpirationLogs.Select(l => l.PolicyId).ToListAsync();

        var expiringPolicies = await db.Policies
            .Where(p => p.EndDate >= yesterday && p.EndDate <= today
                        && !alreadyLogged.Contains(p.Id))
            .ToListAsync();

        foreach (var policy in expiringPolicies)
        {
            _logger.LogInformation("Policy {PolicyId} expired at {EndDate}", policy.Id, policy.EndDate);

            db.PolicyExpirationLogs.Add(new PolicyExpirationLog
            {
                PolicyId = policy.Id,
                LoggedAt = DateTime.Now
            });
        }

        await db.SaveChangesAsync();
    }
}