using EmployeeContactApi.Application.Commands.Dto;
using EmployeeContactApi.Application.Commands.Handlers;
using EmployeeContactApi.Application.Interfaces;
using EmployeeContactApi.Domain.Exceptions;
using EmployeeContactApi.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmployeeContactApi.Tests;

public class CreateEmployeeCommandHandlerTests
{
    private readonly Mock<IEmployeeCommandRepository> _commandRepoMock;
    private readonly Mock<IEmployeeQueryRepository> _queryRepoMock;
    private readonly Mock<ILogger<CreateEmployeeCommandHandler>> _loggerMock;
    private readonly CreateEmployeeCommandHandler _handler;

    public CreateEmployeeCommandHandlerTests()
    {
        _commandRepoMock = new Mock<IEmployeeCommandRepository>();
        _queryRepoMock = new Mock<IEmployeeQueryRepository>();
        _loggerMock = new Mock<ILogger<CreateEmployeeCommandHandler>>();
        _handler = new CreateEmployeeCommandHandler(
            _commandRepoMock.Object,
            _queryRepoMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task HandleBatchAsync_ValidCommands_SavesAndReturnsCount()
    {
        // Arrange
        var commands = new List<CreateEmployeeCommand>
        {
            new("홍길동", "hong@example.com", "01012345678", new DateOnly(2024, 1, 15)),
            new("김철수", "kim@example.com", "01087654321", new DateOnly(2024, 2, 20))
        };

        _queryRepoMock
            .Setup(r => r.FindExistingEmailsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        _commandRepoMock
            .Setup(r => r.SaveAllAsync(It.IsAny<IEnumerable<Employee>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _handler.HandleBatchAsync(commands);

        // Assert
        Assert.Equal(2, result);
        _commandRepoMock.Verify(r => r.SaveAllAsync(It.Is<IEnumerable<Employee>>(e => e.Count() == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleBatchAsync_EmptyList_ReturnsZero()
    {
        // Arrange
        var commands = new List<CreateEmployeeCommand>();

        _queryRepoMock
            .Setup(r => r.FindExistingEmailsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        _commandRepoMock
            .Setup(r => r.SaveAllAsync(It.IsAny<IEnumerable<Employee>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.HandleBatchAsync(commands);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task HandleBatchAsync_DuplicateEmailsInInput_ThrowsException()
    {
        // Arrange
        var commands = new List<CreateEmployeeCommand>
        {
            new("홍길동", "hong@example.com", "01012345678", new DateOnly(2024, 1, 15)),
            new("홍길순", "hong@example.com", "01087654321", new DateOnly(2024, 2, 20))  // Same email
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DuplicateEmailException>(
            () => _handler.HandleBatchAsync(commands));

        Assert.Contains("hong@example.com", ex.DuplicateEmails);
        _commandRepoMock.Verify(r => r.SaveAllAsync(It.IsAny<IEnumerable<Employee>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleBatchAsync_DuplicateEmailsInDatabase_ThrowsException()
    {
        // Arrange
        var commands = new List<CreateEmployeeCommand>
        {
            new("홍길동", "existing@example.com", "01012345678", new DateOnly(2024, 1, 15))
        };

        _queryRepoMock
            .Setup(r => r.FindExistingEmailsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "existing@example.com" });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DuplicateEmailException>(
            () => _handler.HandleBatchAsync(commands));

        Assert.Contains("existing@example.com", ex.DuplicateEmails);
        _commandRepoMock.Verify(r => r.SaveAllAsync(It.IsAny<IEnumerable<Employee>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleBatchAsync_NormalizesPhoneNumber()
    {
        // Arrange
        var commands = new List<CreateEmployeeCommand>
        {
            new("홍길동", "hong@example.com", "010-1234-5678", new DateOnly(2024, 1, 15))
        };

        _queryRepoMock
            .Setup(r => r.FindExistingEmailsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        Employee? savedEmployee = null;
        _commandRepoMock
            .Setup(r => r.SaveAllAsync(It.IsAny<IEnumerable<Employee>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Employee>, CancellationToken>((employees, _) => savedEmployee = employees.First())
            .ReturnsAsync(1);

        // Act
        await _handler.HandleBatchAsync(commands);

        // Assert
        Assert.NotNull(savedEmployee);
        Assert.Equal("01012345678", savedEmployee.Tel);  // Normalized
    }

    [Fact]
    public async Task HandleBatchAsync_PropagatesCancellationToken()
    {
        // Arrange
        var commands = new List<CreateEmployeeCommand>
        {
            new("홍길동", "hong@example.com", "01012345678", new DateOnly(2024, 1, 15))
        };

        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _queryRepoMock
            .Setup(r => r.FindExistingEmailsAsync(It.IsAny<IEnumerable<string>>(), token))
            .ReturnsAsync(new List<string>());

        _commandRepoMock
            .Setup(r => r.SaveAllAsync(It.IsAny<IEnumerable<Employee>>(), token))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleBatchAsync(commands, token);

        // Assert
        _queryRepoMock.Verify(r => r.FindExistingEmailsAsync(It.IsAny<IEnumerable<string>>(), token), Times.Once);
        _commandRepoMock.Verify(r => r.SaveAllAsync(It.IsAny<IEnumerable<Employee>>(), token), Times.Once);
    }
}
