using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EmployeeContactApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeContactApi.Infrastructure.Entities;

[Table("employees")]
[Index(nameof(Name), Name = "idx_employee_name")]
[Index(nameof(Email), Name = "idx_employee_email", IsUnique = true)]
public class EmployeeEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Tel { get; set; } = string.Empty;

    [Required]
    public DateOnly Joined { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Employee ToModel() => new(
        Id: Id,
        Name: Name,
        Email: Email,
        Tel: Tel,
        Joined: Joined,
        CreatedAt: CreatedAt
    );
}
