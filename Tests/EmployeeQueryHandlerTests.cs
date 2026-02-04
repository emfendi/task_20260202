using EmployeeContactApi.Application.Interfaces;
using EmployeeContactApi.Application.Queries.Handlers;
using EmployeeContactApi.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmployeeContactApi.Tests;

public class EmployeeQueryHandlerTests
{
    private readonly Mock<IEmployeeQueryRepository> _repositoryMock;
    private readonly Mock<ILogger<EmployeeQueryHandler>> _loggerMock;
    private readonly EmployeeQueryHandler _handler;

    public EmployeeQueryHandlerTests()
    {
        _repositoryMock = new Mock<IEmployeeQueryRepository>();
        _loggerMock = new Mock<ILogger<EmployeeQueryHandler>>();
        _handler = new EmployeeQueryHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task FindAllAsync_ReturnsPagedResponse()
    {
        // Arrange
        var employees = new List<Employee>
        {
            new(1, "홍길동", "hong@example.com", "01012345678", new DateOnly(2024, 1, 15), DateTime.UtcNow),
            new(2, "김철수", "kim@example.com", "01087654321", new DateOnly(2024, 2, 20), DateTime.UtcNow)
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((employees, 15));

        // Act
        var result = await _handler.FindAllAsync(0, 10);

        // Assert
        Assert.Equal(2, result.Content.Count);
        Assert.Equal(0, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(15, result.TotalElements);
        Assert.Equal(2, result.TotalPages);  // ceil(15/10) = 2
    }

    [Fact]
    public async Task FindAllAsync_CalculatesTotalPagesCorrectly()
    {
        // Arrange
        var employees = new List<Employee>();

        _repositoryMock
            .Setup(r => r.GetAllAsync(0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((employees, 25));

        // Act
        var result = await _handler.FindAllAsync(0, 10);

        // Assert
        Assert.Equal(3, result.TotalPages);  // ceil(25/10) = 3
    }

    [Fact]
    public async Task FindAllAsync_EmptyResult_ReturnsEmptyPage()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAllAsync(0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Employee>(), 0));

        // Act
        var result = await _handler.FindAllAsync(0, 10);

        // Assert
        Assert.Empty(result.Content);
        Assert.Equal(0, result.TotalElements);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public async Task FindByNameAsync_ReturnsMatchingEmployees()
    {
        // Arrange
        var employees = new List<Employee>
        {
            new(1, "홍길동", "hong1@example.com", "01012345678", new DateOnly(2024, 1, 15), DateTime.UtcNow),
            new(3, "홍길동", "hong2@example.com", "01099998888", new DateOnly(2024, 3, 10), DateTime.UtcNow)
        };

        _repositoryMock
            .Setup(r => r.GetByNameAsync("홍길동", It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);

        // Act
        var result = await _handler.FindByNameAsync("홍길동");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal("홍길동", e.Name));
    }

    [Fact]
    public async Task FindByNameAsync_NoMatch_ReturnsEmptyList()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByNameAsync("없는사람", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee>());

        // Act
        var result = await _handler.FindByNameAsync("없는사람");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task FindAllAsync_PropagatesCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _repositoryMock
            .Setup(r => r.GetAllAsync(0, 10, token))
            .ReturnsAsync((new List<Employee>(), 0));

        // Act
        await _handler.FindAllAsync(0, 10, token);

        // Assert
        _repositoryMock.Verify(r => r.GetAllAsync(0, 10, token), Times.Once);
    }

    [Fact]
    public async Task FindByNameAsync_PropagatesCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _repositoryMock
            .Setup(r => r.GetByNameAsync("홍길동", token))
            .ReturnsAsync(new List<Employee>());

        // Act
        await _handler.FindByNameAsync("홍길동", token);

        // Assert
        _repositoryMock.Verify(r => r.GetByNameAsync("홍길동", token), Times.Once);
    }

    [Fact]
    public async Task FindAllAsync_MapsEmployeeToResponse()
    {
        // Arrange
        var employees = new List<Employee>
        {
            new(1, "홍길동", "hong@example.com", "01012345678", new DateOnly(2024, 1, 15), DateTime.UtcNow)
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((employees, 1));

        // Act
        var result = await _handler.FindAllAsync(0, 10);

        // Assert
        var employee = result.Content.First();
        Assert.Equal(1, employee.Id);
        Assert.Equal("홍길동", employee.Name);
        Assert.Equal("hong@example.com", employee.Email);
        Assert.Equal("01012345678", employee.Tel);
        Assert.Equal(new DateOnly(2024, 1, 15), employee.Joined);
    }
}
