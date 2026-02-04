using System.Globalization;
using EmployeeContactApi.Domain.Exceptions;

namespace EmployeeContactApi.Application.Services;

public static class DateParser
{
    private static readonly string[] Formats = { "yyyy.MM.dd", "yyyy-MM-dd", "yyyy/MM/dd" };

    public static DateOnly Parse(string dateStr)
    {
        var trimmed = dateStr.Trim();
        foreach (var format in Formats)
        {
            if (DateOnly.TryParseExact(trimmed, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
        }
        throw InvalidDataFormatException.InvalidDate(trimmed);
    }
}
