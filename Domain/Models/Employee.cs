namespace EmployeeContactApi.Domain.Models;

public record Employee(
    long Id,
    string Name,
    string Email,
    string Tel,
    DateOnly Joined,
    DateTime CreatedAt
);
