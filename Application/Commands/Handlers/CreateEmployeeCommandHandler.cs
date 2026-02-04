using EmployeeContactApi.Application.Commands.Dto;
using EmployeeContactApi.Application.Interfaces;
using EmployeeContactApi.Domain.Exceptions;
using EmployeeContactApi.Domain.Models;
using EmployeeContactApi.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

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

        try
        {
            var count = await _commandRepository.SaveAllAsync(employees, ct);
            _logger.LogInformation("Batch creation completed: {Count} employees created", count);
            return count;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Race condition: another request inserted the same email between check and save
            _logger.LogWarning(ex, "Duplicate email constraint violation during save");
            throw new DuplicateEmailException(["Duplicate email detected (concurrent request)"]);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // Check for common unique constraint violation patterns across different databases
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("IX_") || message.Contains("idx_");
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
