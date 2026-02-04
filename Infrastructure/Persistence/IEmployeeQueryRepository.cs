using EmployeeContactApi.Domain.Models;

namespace EmployeeContactApi.Infrastructure.Persistence;

public interface IEmployeeQueryRepository
{
    Task<(List<Employee> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<List<Employee>> GetByNameAsync(string name, CancellationToken ct = default);
    Task<List<string>> FindExistingEmailsAsync(IEnumerable<string> emails, CancellationToken ct = default);
}
