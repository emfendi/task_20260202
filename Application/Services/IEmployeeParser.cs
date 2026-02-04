using EmployeeContactApi.Application.Commands.Dto;

namespace EmployeeContactApi.Application.Services;

public interface IEmployeeParser
{
    List<CreateEmployeeCommand> Parse(string content);
    bool CanParse(string? contentType, string? filename);
    bool CanParseContent(string content) => false;
}
