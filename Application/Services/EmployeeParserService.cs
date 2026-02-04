using EmployeeContactApi.Application.Commands.Dto;
using EmployeeContactApi.Domain.Exceptions;

namespace EmployeeContactApi.Application.Services;

public class EmployeeParserService
{
    private readonly IEnumerable<IEmployeeParser> _parsers;

    public EmployeeParserService(IEnumerable<IEmployeeParser> parsers)
    {
        _parsers = parsers;
    }

    public List<CreateEmployeeCommand> Parse(string content, string? contentType, string? filename)
    {
        var parser = FindParser(contentType, filename, content);
        return parser.Parse(content);
    }

    private IEmployeeParser FindParser(string? contentType, string? filename, string content)
    {
        // First try to find by contentType or filename
        var parser = _parsers.FirstOrDefault(p => p.CanParse(contentType, filename));
        if (parser != null) return parser;

        // Fallback: auto-detect by content
        parser = _parsers.FirstOrDefault(p => p.CanParseContent(content));
        if (parser != null) return parser;

        throw new InvalidDataFormatException("Unsupported file format");
    }
}
