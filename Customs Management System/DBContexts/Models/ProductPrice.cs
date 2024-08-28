using System;
using System.Collections.Generic;

namespace Customs_Management_System.DBContexts.Models;

public partial class ProductPrice
{
    public int PriceId { get; set; }

    public string? Category { get; set; }

    public string? ProductName { get; set; }

    public decimal? Price { get; set; }

    public string? HsCode { get; set; }
}
