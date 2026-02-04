using System.ComponentModel.DataAnnotations;

namespace EmployeeContactApi.Presentation.Dto;

public record CreateEmployeeRequest(
    [Required(ErrorMessage = "Name is required")]
    string Name,

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email,

    [Required(ErrorMessage = "Phone number is required")]
    string Tel,

    [Required(ErrorMessage = "Joined date is required")]
    DateOnly Joined
);
