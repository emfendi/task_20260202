using EmployeeContactApi.Domain.Exceptions;

namespace EmployeeContactApi.Tests;

public class DuplicateEmailExceptionTests
{
    [Fact]
    public void Constructor_SingleEmail_StoresEmail()
    {
        // Arrange & Act
        var ex = new DuplicateEmailException(["test@example.com"]);

        // Assert
        Assert.Single(ex.DuplicateEmails);
        Assert.Contains("test@example.com", ex.DuplicateEmails);
    }

    [Fact]
    public void Constructor_MultipleEmails_StoresAllEmails()
    {
        // Arrange
        var emails = new List<string> { "a@test.com", "b@test.com", "c@test.com" };

        // Act
        var ex = new DuplicateEmailException(emails);

        // Assert
        Assert.Equal(3, ex.DuplicateEmails.Count);
        Assert.Contains("a@test.com", ex.DuplicateEmails);
        Assert.Contains("b@test.com", ex.DuplicateEmails);
        Assert.Contains("c@test.com", ex.DuplicateEmails);
    }

    [Fact]
    public void Message_ContainsDuplicateInfo()
    {
        // Arrange & Act
        var ex = new DuplicateEmailException(["test@example.com"]);

        // Assert
        Assert.Contains("Duplicate", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
