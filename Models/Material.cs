using System;
using System.Collections.Generic;

namespace japantune.Models;

public partial class Material
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int Price { get; set; }

    public int Quantity { get; set; }

    public int SupplierId { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Supplier Supplier { get; set; } = null!;
}
