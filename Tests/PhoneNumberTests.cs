using EmployeeContactApi.Domain.Exceptions;
using EmployeeContactApi.Domain.ValueObjects;

namespace EmployeeContactApi.Tests;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("01012345678")]
    [InlineData("01112345678")]
    [InlineData("010-1234-5678")]
    [InlineData("011-123-4567")]
    [InlineData("010 1234 5678")]
    public void Constructor_ValidPhoneNumber_CreatesInstance(string input)
    {
        // Act
        var phoneNumber = new PhoneNumber(input);

        // Assert
        Assert.Matches(@"^01\d{8,9}$", phoneNumber.Value);
    }

    [Theory]
    [InlineData("01012345678", "01012345678")]
    [InlineData("010-1234-5678", "01012345678")]
    [InlineData("010 1234 5678", "01012345678")]
    [InlineData("010-123-4567", "0101234567")]
    public void Constructor_NormalizesPhoneNumber(string input, string expected)
    {
        // Act
        var phoneNumber = new PhoneNumber(input);

        // Assert
        Assert.Equal(expected, phoneNumber.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyOrNull_ThrowsException(string? input)
    {
        // Act & Assert
        Assert.Throws<InvalidDataFormatException>(() => new PhoneNumber(input!));
    }

    [Theory]
    [InlineData("0212345678")]  // Not starting with 01
    [InlineData("0101234567890")]  // Too long
    [InlineData("010123456")]  // Too short
    [InlineData("abcdefghijk")]  // Non-numeric
    public void Constructor_InvalidFormat_ThrowsException(string input)
    {
        // Act & Assert
        var ex = Assert.Throws<InvalidDataFormatException>(() => new PhoneNumber(input));
        Assert.Contains("phone", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        // Arrange
        var phone1 = new PhoneNumber("010-1234-5678");
        var phone2 = new PhoneNumber("01012345678");

        // Assert
        Assert.Equal(phone1, phone2);
        Assert.Equal(phone1.GetHashCode(), phone2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("010-1234-5678");

        // Act & Assert
        Assert.Equal("01012345678", phoneNumber.ToString());
    }
}
