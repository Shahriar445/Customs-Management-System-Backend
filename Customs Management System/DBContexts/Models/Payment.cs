﻿using System;
using System.Collections.Generic;

namespace Customs_Management_System.DBContexts.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int UserId { get; set; }

    public int DeclarationId { get; set; }

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public string Status { get; set; } = null!;

    public int? ProductId { get; set; }

    public virtual Declaration Declaration { get; set; } = null!;

    public virtual Product? Product { get; set; }

    public virtual User User { get; set; } = null!;
}
