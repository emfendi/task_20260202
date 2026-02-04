using EmployeeContactApi.Application.Services;
using EmployeeContactApi.Domain.Exceptions;
using Xunit;

namespace EmployeeContactApi.Tests;

public class DateParserTests
{
    [Theory]
    [InlineData("2018.03.07")]
    [InlineData("2018-03-07")]
    [InlineData("2018/03/07")]
    public void Parse_ValidFormats_ReturnsDateOnly(string dateStr)
    {
        // Act
        var result = DateParser.Parse(dateStr);

        // Assert
        Assert.Equal(2018, result.Year);
        Assert.Equal(3, result.Month);
        Assert.Equal(7, result.Day);
    }

    [Fact]
    public void Parse_InvalidFormat_ThrowsException()
    {
        // Arrange
        var invalidDate = "07-03-2018";

        // Act & Assert
        Assert.Throws<InvalidDataFormatException>(() => DateParser.Parse(invalidDate));
    }

    [Fact]
    public void Parse_TrimsWhitespace()
    {
        // Arrange
        var dateWithSpaces = "  2018.03.07  ";

        // Act
        var result = DateParser.Parse(dateWithSpaces);

        // Assert
        Assert.Equal(new DateOnly(2018, 3, 7), result);
    }
}
