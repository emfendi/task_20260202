using System.Text.Json;
using EmployeeContactApi.Application.Commands.Dto;
using EmployeeContactApi.Domain.Exceptions;

namespace EmployeeContactApi.Application.Services;

public class JsonParser : IEmployeeParser
{
    private readonly ILogger<JsonParser> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public JsonParser(ILogger<JsonParser> logger)
    {
        _logger = logger;
    }

    public List<CreateEmployeeCommand> Parse(string content)
    {
        _logger.LogDebug("Parsing JSON content");

        var trimmed = content.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw InvalidDataFormatException.InvalidJson("Empty content");

        try
        {
            List<EmployeeJsonDto> dtos;
            if (trimmed.StartsWith('['))
            {
                dtos = JsonSerializer.Deserialize<List<EmployeeJsonDto>>(trimmed, JsonOptions) ?? [];
            }
            else
            {
                var single = JsonSerializer.Deserialize<EmployeeJsonDto>(trimmed, JsonOptions);
                dtos = single != null ? [single] : [];
            }

            var result = dtos.Select(dto => new CreateEmployeeCommand(
                name: dto.Name.Trim(),
                email: dto.Email.Trim(),
                tel: dto.Tel.Trim(),
                joined: DateParser.Parse(dto.Joined)
            )).ToList();

            _logger.LogInformation("Successfully parsed {Count} records from JSON", result.Count);
            return result;
        }
        catch (JsonException e)
        {
            _logger.LogError("Failed to parse JSON: {Message}", e.Message);
            throw InvalidDataFormatException.InvalidJson(e.Message);
        }
    }

    public bool CanParse(string? contentType, string? filename)
    {
        return contentType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true ||
               filename?.EndsWith(".json", StringComparison.OrdinalIgnoreCase) == true;
    }

    public bool CanParseContent(string content)
    {
        var trimmed = content.Trim();
        return trimmed.StartsWith('[') || trimmed.StartsWith('{');
    }

    private record EmployeeJsonDto(string Name, string Email, string Tel, string Joined);
}
