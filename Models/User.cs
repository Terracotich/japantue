using System;
using System.Collections.Generic;

namespace japantune.Models;

public partial class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string SurName { get; set; } = null!;

    public string? LastName { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string ClientLogin { get; set; } = null!;

    public string ClientPassword { get; set; } = null!;

    public string? CardNum { get; set; }

    public int RoleId { get; set; }

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Role Role { get; set; } = null!;
}
