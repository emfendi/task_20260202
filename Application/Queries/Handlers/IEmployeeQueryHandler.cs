using EmployeeContactApi.Application.Queries.Dto;

namespace EmployeeContactApi.Application.Queries.Handlers;

public interface IEmployeeQueryHandler
{
    Task<EmployeePageResponse> FindAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<List<EmployeeResponse>> FindByNameAsync(string name, CancellationToken ct = default);
}
