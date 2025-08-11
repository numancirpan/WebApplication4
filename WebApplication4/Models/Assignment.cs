// Models/Assignment.cs
using System;
using System.ComponentModel.DataAnnotations;

public class Assignment
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [Required]
    public int EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }

    public DateTime AssignmentDate { get; set; } = DateTime.Now;
    public DateTime? ReturnDate { get; set; }

    // Accessories
    public bool HasCharger { get; set; }
    public bool HasHeadset { get; set; }
    public bool HasDeckStation { get; set; }
    public bool HasWirelessHeadset { get; set; }
    public bool HasKeyboard { get; set; }
    public bool HasLaptopBag { get; set; }
    public bool HasMouse { get; set; }
    public bool HasJabraSpeak { get; set; }

    public string? OtherAccessories { get; set; }
    public string? Notes { get; set; }

    [Required]
    public string Status { get; set; } = "Active"; // Active, Returned, Lost, etc.
}