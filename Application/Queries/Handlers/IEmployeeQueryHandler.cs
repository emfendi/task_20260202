using EmployeeContactApi.Application.Queries.Dto;

namespace EmployeeContactApi.Application.Queries.Handlers;

public interface IEmployeeQueryHandler
{
    Task<EmployeePageResponse> FindAllAsync(int page, int pageSize);
    Task<List<EmployeeResponse>> FindByNameAsync(string name);
}
