using System;
using System.Collections.Generic;

namespace japantune.Models;

public partial class Supplier
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Country { get; set; } = null!;

    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
}
