using EmployeeContactApi.Domain.Models;

namespace EmployeeContactApi.Application.Interfaces;

public interface IEmployeeCommandRepository
{
    Task<int> SaveAllAsync(IEnumerable<Employee> employees, CancellationToken ct = default);
}
