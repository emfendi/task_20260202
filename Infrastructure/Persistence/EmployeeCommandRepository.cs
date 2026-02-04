using EmployeeContactApi.Domain.Models;
using EmployeeContactApi.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeContactApi.Infrastructure.Persistence;

public class EmployeeCommandRepository : IEmployeeCommandRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmployeeCommandRepository> _logger;

    public EmployeeCommandRepository(AppDbContext context, ILogger<EmployeeCommandRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> SaveAllAsync(IEnumerable<Employee> employees)
    {
        var employeeList = employees.ToList();
        if (employeeList.Count == 0) return 0;

        var entities = employeeList.Select(ToEntity).ToList();
        await _context.Employees.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Saved {Count} employees", entities.Count);
        return entities.Count;
    }

    private static EmployeeEntity ToEntity(Employee employee)
    {
        return new EmployeeEntity
        {
            Name = employee.Name,
            Email = employee.Email,
            Tel = employee.Tel,
            Joined = employee.Joined,
            CreatedAt = DateTime.UtcNow
        };
    }
}
