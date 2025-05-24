using System;
using System.Collections.Generic;

namespace japantune.Models;

public partial class Car
{
    public int Id { get; set; }

    public string Mark { get; set; } = null!;

    public string Model { get; set; } = null!;

    public int ReleaseDate { get; set; }

    public string? LicensePlate { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
