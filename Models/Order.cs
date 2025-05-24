using System;
using System.Collections.Generic;

namespace japantune.Models;

public partial class Order
{
    public int Id { get; set; }

    public DateOnly OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public int PaymentId { get; set; }

    public int UserId { get; set; }

    public int MaterialId { get; set; }

    public int? ReviewId { get; set; }

    public virtual Material Material { get; set; } = null!;

    public virtual Payment Payment { get; set; } = null!;

    public virtual Review? Review { get; set; }

    public virtual User User { get; set; } = null!;
}
