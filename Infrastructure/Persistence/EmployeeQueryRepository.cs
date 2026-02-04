using EmployeeContactApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeContactApi.Infrastructure.Persistence;

public class EmployeeQueryRepository : IEmployeeQueryRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmployeeQueryRepository> _logger;

    public EmployeeQueryRepository(AppDbContext context, ILogger<EmployeeQueryRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<Employee> Items, int TotalCount)> GetAllAsync(int page, int pageSize)
    {
        _logger.LogDebug("Querying all employees: page={Page}, pageSize={PageSize}", page, pageSize);

        var totalCount = await _context.Employees.CountAsync();
        var entities = await _context.Employees
            .OrderBy(e => e.Id)
            .Skip(page * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        var items = entities.Select(e => e.ToModel()).ToList();

        _logger.LogDebug("Query result: {Count} employees, total={Total}", items.Count, totalCount);
        return (items, totalCount);
    }

    public async Task<List<Employee>> GetByNameAsync(string name)
    {
        _logger.LogDebug("Querying employees by name: {Name}", name);

        var entities = await _context.Employees
            .Where(e => e.Name == name)
            .AsNoTracking()
            .ToListAsync();

        var employees = entities.Select(e => e.ToModel()).ToList();

        _logger.LogDebug("Found {Count} employee(s) with name: {Name}", employees.Count, name);
        return employees;
    }

    public async Task<List<string>> FindExistingEmailsAsync(IEnumerable<string> emails)
    {
        var emailList = emails.ToList();
        _logger.LogDebug("Checking for existing emails: {Count} emails", emailList.Count);

        var existingEmails = await _context.Employees
            .Where(e => emailList.Contains(e.Email))
            .Select(e => e.Email)
            .ToListAsync();

        _logger.LogDebug("Found {Count} existing emails in database", existingEmails.Count);
        return existingEmails;
    }
}
