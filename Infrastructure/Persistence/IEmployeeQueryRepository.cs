using EmployeeContactApi.Domain.Models;

namespace EmployeeContactApi.Infrastructure.Persistence;

public interface IEmployeeQueryRepository
{
    Task<(List<Employee> Items, int TotalCount)> GetAllAsync(int page, int pageSize);
    Task<List<Employee>> GetByNameAsync(string name);
    Task<List<string>> FindExistingEmailsAsync(IEnumerable<string> emails);
}
