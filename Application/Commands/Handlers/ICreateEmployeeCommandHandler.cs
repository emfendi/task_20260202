using EmployeeContactApi.Application.Commands.Dto;

namespace EmployeeContactApi.Application.Commands.Handlers;

public interface ICreateEmployeeCommandHandler
{
    Task<int> HandleBatchAsync(IEnumerable<CreateEmployeeCommand> commands, CancellationToken ct = default);
}
