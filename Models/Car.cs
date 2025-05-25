using System.ComponentModel.DataAnnotations;

namespace japantune.Models;

public partial class Car
{
    public int Id { get; set; }

    [Required]
    public string Mark { get; set; } = null!;

    [Required]
    public string Model { get; set; } = null!;

    [Range(1900, 2100, ErrorMessage = "Год выпуска должен быть между 1900 и 2100")]
    public int ReleaseDate { get; set; } // Просто год (число)

    public string? LicensePlate { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}