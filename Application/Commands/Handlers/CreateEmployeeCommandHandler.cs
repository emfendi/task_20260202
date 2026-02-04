using EmployeeContactApi.Application.Commands.Dto;
using EmployeeContactApi.Domain.Exceptions;
using EmployeeContactApi.Domain.Models;
using EmployeeContactApi.Domain.ValueObjects;
using EmployeeContactApi.Infrastructure.Persistence;

namespace EmployeeContactApi.Application.Commands.Handlers;

public class CreateEmployeeCommandHandler : ICreateEmployeeCommandHandler
{
    private readonly IEmployeeCommandRepository _commandRepository;
    private readonly IEmployeeQueryRepository _queryRepository;
    private readonly ILogger<CreateEmployeeCommandHandler> _logger;

    public CreateEmployeeCommandHandler(
        IEmployeeCommandRepository commandRepository,
        IEmployeeQueryRepository queryRepository,
        ILogger<CreateEmployeeCommandHandler> logger)
    {
        _commandRepository = commandRepository;
        _queryRepository = queryRepository;
        _logger = logger;
    }

    public async Task<int> HandleBatchAsync(IEnumerable<CreateEmployeeCommand> commands, CancellationToken ct = default)
    {
        var commandList = commands.ToList();
        _logger.LogDebug("Creating {Count} employees in batch", commandList.Count);

        // Check for duplicates within the input
        var inputEmails = commandList.Select(c => c.Email).ToList();
        var inputDuplicates = inputEmails.GroupBy(e => e)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (inputDuplicates.Count > 0)
        {
            _logger.LogWarning("Duplicate emails in input: {Emails}", string.Join(", ", inputDuplicates));
            throw new DuplicateEmailException(inputDuplicates);
        }

        // Check for duplicates against existing data
        var existingEmails = await _queryRepository.FindExistingEmailsAsync(inputEmails, ct);

        if (existingEmails.Count > 0)
        {
            _logger.LogWarning("Duplicate emails in database: {Emails}", string.Join(", ", existingEmails));
            throw new DuplicateEmailException(existingEmails);
        }

        var employees = commandList.Select(CreateEmployee).ToList();
        var count = await _commandRepository.SaveAllAsync(employees, ct);

        _logger.LogInformation("Batch creation completed: {Count} employees created", count);
        return count;
    }

    private static Employee CreateEmployee(CreateEmployeeCommand command)
    {
        var phoneNumber = new PhoneNumber(command.Tel);
        return new Employee(
            Id: 0,
            Name: command.Name,
            Email: command.Email,
            Tel: phoneNumber.Value,
            Joined: command.Joined,
            CreatedAt: DateTime.UtcNow
        );
    }
}
