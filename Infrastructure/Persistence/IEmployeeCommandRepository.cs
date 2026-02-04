using EmployeeContactApi.Domain.Models;

namespace EmployeeContactApi.Infrastructure.Persistence;

public interface IEmployeeCommandRepository
{
    Task<int> SaveAllAsync(IEnumerable<Employee> employees);
}
