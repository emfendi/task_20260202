namespace EmployeeContactApi.Domain.Exceptions;

public class InvalidDataFormatException : Exception
{
    public InvalidDataFormatException(string message) : base(message) { }
    public InvalidDataFormatException(string message, Exception? innerException) : base(message, innerException) { }

    public static InvalidDataFormatException InvalidCsv(string reason) =>
        new($"Invalid CSV format: {reason}");

    public static InvalidDataFormatException InvalidJson(string reason) =>
        new($"Invalid JSON format: {reason}");

    public static InvalidDataFormatException InvalidDate(string dateString) =>
        new($"Invalid date format: {dateString}. Expected formats: yyyy.MM.dd or yyyy-MM-dd");

    public static InvalidDataFormatException InvalidField(string field, string reason) =>
        new($"Invalid {field}: {reason}");
}
