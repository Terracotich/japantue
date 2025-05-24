using System;
using System.Collections.Generic;

namespace japantune.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int Price { get; set; }

    public string PayMethod { get; set; } = null!;

    public DateOnly PaymentDate { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User User { get; set; } = null!;
}
