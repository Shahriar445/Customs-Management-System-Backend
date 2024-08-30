using System;
using System.Collections.Generic;

namespace Customs_Management_System.DBContexts.Models;

public partial class ShipmentDetail
{
    public int ShipmentDetailId { get; set; }

    public string Country { get; set; } = null!;

    public string Port { get; set; } = null!;

    public decimal? Vat { get; set; }

    public decimal? Tax { get; set; }
}
