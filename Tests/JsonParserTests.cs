using EmployeeContactApi.Application.Services;
using EmployeeContactApi.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EmployeeContactApi.Tests;

public class JsonParserTests
{
    private readonly JsonParser _parser;

    public JsonParserTests()
    {
        var logger = Mock.Of<ILogger<JsonParser>>();
        _parser = new JsonParser(logger);
    }

    [Fact]
    public void Parse_ValidJsonArray_ReturnsCommands()
    {
        // Arrange
        var json = @"[{""name"":""김클로"", ""email"":""clo@example.com"", ""tel"":""010-1111-2424"", ""joined"":""2012-01-05""}]";

        // Act
        var result = _parser.Parse(json);

        // Assert
        Assert.Single(result);
        Assert.Equal("김클로", result[0].Name);
    }

    [Fact]
    public void Parse_SingleJsonObject_ReturnsOneCommand()
    {
        // Arrange
        var json = @"{""name"":""김클로"", ""email"":""clo@example.com"", ""tel"":""010-1111-2424"", ""joined"":""2012-01-05""}";

        // Act
        var result = _parser.Parse(json);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void Parse_EmptyContent_ThrowsException()
    {
        // Arrange
        var json = "";

        // Act & Assert
        Assert.Throws<InvalidDataFormatException>(() => _parser.Parse(json));
    }

    [Fact]
    public void Parse_InvalidJson_ThrowsException()
    {
        // Arrange
        var json = "{ invalid json }";

        // Act & Assert
        Assert.Throws<InvalidDataFormatException>(() => _parser.Parse(json));
    }

    [Fact]
    public void CanParse_JsonContentType_ReturnsTrue()
    {
        Assert.True(_parser.CanParse("application/json", null));
        Assert.True(_parser.CanParse(null, "employees.json"));
    }
}
