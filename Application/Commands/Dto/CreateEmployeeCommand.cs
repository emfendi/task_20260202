using System.Text.RegularExpressions;
using EmployeeContactApi.Domain.Exceptions;
using EmployeeContactApi.Domain.ValueObjects;

namespace EmployeeContactApi.Application.Commands.Dto;

public record CreateEmployeeCommand
{
    private static readonly Regex EmailRegex = new(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$", RegexOptions.Compiled);

    public string Name { get; init; }
    public string Email { get; init; }
    public string Tel { get; init; }
    public DateOnly Joined { get; init; }

    public CreateEmployeeCommand(string name, string email, string tel, DateOnly joined)
    {
        ValidateName(name);
        ValidateEmail(email);
        ValidatePhoneNumber(tel);

        Name = name.Trim();
        Email = email.Trim();
        Tel = tel.Trim();
        Joined = joined;
    }

    private static void ValidateName(string name)
    {
        var trimmed = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw InvalidDataFormatException.InvalidField("name", "Name cannot be blank");
        if (trimmed.Length > 100)
            throw InvalidDataFormatException.InvalidField("name", "Name too long (max 100 characters)");
    }

    private static void ValidateEmail(string email)
    {
        var trimmed = email.Trim();
        if (!EmailRegex.IsMatch(trimmed))
            throw InvalidDataFormatException.InvalidField("email", $"Invalid email format: {trimmed}");
    }

    private static void ValidatePhoneNumber(string tel)
    {
        // Validation is performed by PhoneNumber Value Object
        _ = new PhoneNumber(tel);
    }
}
