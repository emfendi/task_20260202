using EmployeeContactApi.Application.Commands.Dto;
using EmployeeContactApi.Domain.Exceptions;

namespace EmployeeContactApi.Application.Services;

public class CsvParser : IEmployeeParser
{
    private readonly ILogger<CsvParser> _logger;

    public CsvParser(ILogger<CsvParser> logger)
    {
        _logger = logger;
    }

    public List<CreateEmployeeCommand> Parse(string content)
    {
        _logger.LogDebug("Parsing CSV content");

        var lines = content.Split('\n')
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        if (lines.Count == 0)
            throw InvalidDataFormatException.InvalidCsv("Empty content");

        var result = new List<CreateEmployeeCommand>();
        for (int i = 0; i < lines.Count; i++)
        {
            try
            {
                result.Add(ParseLine(lines[i]));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to parse line {LineNumber}: {Line}", i + 1, lines[i]);
                throw InvalidDataFormatException.InvalidCsv($"Error at line {i + 1}: {e.Message}");
            }
        }

        _logger.LogInformation("Successfully parsed {Count} records from CSV", result.Count);
        return result;
    }

    private static CreateEmployeeCommand ParseLine(string line)
    {
        var parts = line.Split(',').Select(p => p.Trim()).ToArray();

        if (parts.Length < 4)
            throw InvalidDataFormatException.InvalidCsv($"Expected 4 fields (name, email, tel, joined), got {parts.Length}");

        return new CreateEmployeeCommand(
            name: parts[0],
            email: parts[1],
            tel: parts[2],
            joined: DateParser.Parse(parts[3])
        );
    }

    public bool CanParse(string? contentType, string? filename)
    {
        return contentType?.Contains("csv", StringComparison.OrdinalIgnoreCase) == true ||
               contentType?.Contains("text/plain", StringComparison.OrdinalIgnoreCase) == true ||
               filename?.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) == true;
    }

    public bool CanParseContent(string content)
    {
        var trimmed = content.Trim();
        return !trimmed.StartsWith('[') && !trimmed.StartsWith('{');
    }
}
