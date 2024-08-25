using System;
using System.Collections.Generic;

namespace Customs_Management_System.DBContexts.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public int DeclarationId { get; set; }

    public string ProductName { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal Weight { get; set; }

    public string? CountryOfOrigin { get; set; }

    public string? Hscode { get; set; }

    public string? Category { get; set; }

    public decimal? TotalPrice { get; set; }

    public bool? IsPayment { get; set; }

    public virtual Declaration Declaration { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
