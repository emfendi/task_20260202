namespace EmployeeContactApi.Application.Queries.Dto;

public record EmployeePageResponse(
    List<EmployeeResponse> Content,
    int Page,
    int PageSize,
    long TotalElements,
    int TotalPages
);
