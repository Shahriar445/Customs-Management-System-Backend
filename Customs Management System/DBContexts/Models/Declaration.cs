using System;
using System.Collections.Generic;

namespace Customs_Management_System.DBContexts.Models;

public partial class Declaration
{
    public int DeclarationId { get; set; }

    public int UserId { get; set; }

    public DateTime DeclarationDate { get; set; }

    public string Status { get; set; } = null!;

    public int RoleId { get; set; }

    public virtual ICollection<Monitoring> Monitorings { get; set; } = new List<Monitoring>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

    public virtual User User { get; set; } = null!;
}
