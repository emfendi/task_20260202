using System.Text.RegularExpressions;
using EmployeeContactApi.Domain.Exceptions;

namespace EmployeeContactApi.Domain.ValueObjects;

public readonly record struct PhoneNumber
{
    private static readonly Regex ValidationRegex = new(@"^01\d{8,9}$", RegexOptions.Compiled);

    public string Value { get; }

    public PhoneNumber(string rawPhoneNumber)
    {
        if (string.IsNullOrWhiteSpace(rawPhoneNumber))
            throw InvalidDataFormatException.InvalidField("tel", "Phone number cannot be blank");

        var trimmed = rawPhoneNumber.Trim();
        var normalized = Normalize(trimmed);

        if (!ValidationRegex.IsMatch(normalized))
            throw InvalidDataFormatException.InvalidField("tel", $"Invalid phone number format: {trimmed}");

        Value = normalized;
    }

    private static string Normalize(string phoneNumber)
    {
        return phoneNumber.Replace("-", "").Replace(" ", "");
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}
