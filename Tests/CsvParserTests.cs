using EmployeeContactApi.Application.Services;
using EmployeeContactApi.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EmployeeContactApi.Tests;

public class CsvParserTests
{
    private readonly CsvParser _parser;

    public CsvParserTests()
    {
        var logger = Mock.Of<ILogger<CsvParser>>();
        _parser = new CsvParser(logger);
    }

    [Fact]
    public void Parse_ValidCsv_ReturnsCommands()
    {
        // Arrange
        var csv = "김철수, charles@example.com, 01075312468, 2018.03.07";

        // Act
        var result = _parser.Parse(csv);

        // Assert
        Assert.Single(result);
        Assert.Equal("김철수", result[0].Name);
        Assert.Equal("charles@example.com", result[0].Email);
    }

    [Fact]
    public void Parse_MultipleLines_ReturnsAllCommands()
    {
        // Arrange
        var csv = @"김철수, charles@example.com, 01075312468, 2018.03.07
박영희, matilda@example.com, 01087654321, 2021.04.28";

        // Act
        var result = _parser.Parse(csv);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Parse_EmptyContent_ThrowsException()
    {
        // Arrange
        var csv = "";

        // Act & Assert
        Assert.Throws<InvalidDataFormatException>(() => _parser.Parse(csv));
    }

    [Fact]
    public void Parse_InsufficientFields_ThrowsException()
    {
        // Arrange
        var csv = "김철수, charles@example.com";

        // Act & Assert
        Assert.Throws<InvalidDataFormatException>(() => _parser.Parse(csv));
    }

    [Fact]
    public void CanParse_CsvContentType_ReturnsTrue()
    {
        Assert.True(_parser.CanParse("text/csv", null));
        Assert.True(_parser.CanParse("text/plain", null));
        Assert.True(_parser.CanParse(null, "employees.csv"));
    }

    [Fact]
    public void CanParse_JsonContentType_ReturnsFalse()
    {
        Assert.False(_parser.CanParse("application/json", null));
    }
}
