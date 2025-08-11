// Models/Equipment.cs
using System.ComponentModel.DataAnnotations;

public class Equipment
{
    public int Id { get; set; }

    [Required]
    public string? EquipmentName { get; set; }

    public string? AssetNumber { get; set; }
    public string? ServiceTag { get; set; }
    public string? Status { get; set; }
    public string? EquipmentType { get; set; }
    public string? SerialOrIMEI { get; set; }
    public int? ModelYear { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Assignment>? Assignments { get; set; }

}