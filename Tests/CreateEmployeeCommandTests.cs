using EmployeeContactApi.Application.Commands.Dto;
using EmployeeContactApi.Domain.Exceptions;

namespace EmployeeContactApi.Tests;

public class CreateEmployeeCommandTests
{
    [Fact]
    public void Constructor_ValidInput_CreatesCommand()
    {
        // Arrange & Act
        var command = new CreateEmployeeCommand(
            name: "홍길동",
            email: "hong@example.com",
            tel: "01012345678",
            joined: new DateOnly(2024, 1, 15)
        );

        // Assert
        Assert.Equal("홍길동", command.Name);
        Assert.Equal("hong@example.com", command.Email);
        Assert.Equal("01012345678", command.Tel);
        Assert.Equal(new DateOnly(2024, 1, 15), command.Joined);
    }

    [Fact]
    public void Constructor_TrimsWhitespace()
    {
        // Arrange & Act
        var command = new CreateEmployeeCommand(
            name: "  홍길동  ",
            email: "  hong@example.com  ",
            tel: "  01012345678  ",
            joined: new DateOnly(2024, 1, 15)
        );

        // Assert
        Assert.Equal("홍길동", command.Name);
        Assert.Equal("hong@example.com", command.Email);
        Assert.Equal("01012345678", command.Tel);
    }

    [Theory]
    [InlineData("", "hong@example.com", "01012345678")]
    [InlineData("   ", "hong@example.com", "01012345678")]
    public void Constructor_EmptyName_ThrowsException(string name, string email, string tel)
    {
        // Act & Assert
        Assert.Throws<InvalidDataFormatException>(() =>
            new CreateEmployeeCommand(name, email, tel, new DateOnly(2024, 1, 15)));
    }

    [Theory]
    [InlineData("홍길동", "", "01012345678")]
    [InlineData("홍길동", "   ", "01012345678")]
    [InlineData("홍길동", "invalid-email", "01012345678")]
    [InlineData("홍길동", "no-at-sign.com", "01012345678")]
    public void Constructor_InvalidEmail_ThrowsException(string name, string email, string tel)
    {
        // Act & Assert
        Assert.Throws<InvalidDataFormatException>(() =>
            new CreateEmployeeCommand(name, email, tel, new DateOnly(2024, 1, 15)));
    }

    [Theory]
    [InlineData("홍길동", "hong@example.com", "")]
    [InlineData("홍길동", "hong@example.com", "invalid")]
    [InlineData("홍길동", "hong@example.com", "0212345678")]
    public void Constructor_InvalidPhone_ThrowsException(string name, string email, string tel)
    {
        // Act & Assert
        Assert.Throws<InvalidDataFormatException>(() =>
            new CreateEmployeeCommand(name, email, tel, new DateOnly(2024, 1, 15)));
    }
}
