namespace EmployeeContactApi.Presentation.Dto;

public record ErrorResponse(
    int Status,
    string Error,
    string Message,
    string? Path = null
);
