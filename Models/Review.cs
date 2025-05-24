using System;
using System.Collections.Generic;

namespace japantune.Models;

public partial class Review
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public short Rating { get; set; }

    public DateOnly ReviewDate { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User User { get; set; } = null!;
}
