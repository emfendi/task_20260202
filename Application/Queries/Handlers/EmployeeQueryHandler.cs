using EmployeeContactApi.Application.Queries.Dto;
using EmployeeContactApi.Infrastructure.Persistence;

namespace EmployeeContactApi.Application.Queries.Handlers;

public class EmployeeQueryHandler : IEmployeeQueryHandler
{
    private readonly IEmployeeQueryRepository _repository;
    private readonly ILogger<EmployeeQueryHandler> _logger;

    public EmployeeQueryHandler(IEmployeeQueryRepository repository, ILogger<EmployeeQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<EmployeePageResponse> FindAllAsync(int page, int pageSize)
    {
        _logger.LogDebug("Querying all employees: page={Page}, pageSize={PageSize}", page, pageSize);

        var (items, totalCount) = await _repository.GetAllAsync(page, pageSize);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var response = new EmployeePageResponse(
            Content: items.Select(e => EmployeeResponse.From(e)).ToList(),
            Page: page,
            PageSize: pageSize,
            TotalElements: totalCount,
            TotalPages: totalPages
        );

        _logger.LogDebug("Query result: {Count} employees, total={Total}", response.Content.Count, response.TotalElements);
        return response;
    }

    public async Task<List<EmployeeResponse>> FindByNameAsync(string name)
    {
        _logger.LogDebug("Querying employees by name: {Name}", name);

        var employees = await _repository.GetByNameAsync(name);
        var result = employees.Select(e => EmployeeResponse.From(e)).ToList();

        _logger.LogDebug("Found {Count} employee(s) with name: {Name}", result.Count, name);
        return result;
    }
}
