using EmployeeContactApi.Domain.Models;

namespace EmployeeContactApi.Application.Queries.Dto;

public record EmployeeResponse(
    long Id,
    string Name,
    string Email,
    string Tel,
    DateOnly Joined
)
{
    public static EmployeeResponse From(Employee employee) => new(
        Id: employee.Id,
        Name: employee.Name,
        Email: employee.Email,
        Tel: employee.Tel,
        Joined: employee.Joined
    );
}
