// Models/Employee.cs
using System.ComponentModel.DataAnnotations;

public class Employee
{
    public int Id { get; set; }
    [Required] // Bu satırı ekleyin
    public string? FullName { get; set; }  // string? yerine string yapın
    public string? Location { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Note { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; }
    public string? Email { get; set; }

    public ICollection<Assignment>? Assignments { get; set; }

}