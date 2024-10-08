using System;
using System.Collections.Generic;

namespace Customs_Management_System.DBContexts.Models;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public DateTime InvoiceDate { get; set; }

    public int PaymentId { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Currency { get; set; }

    public int? DeclarationId { get; set; }

    public virtual User User { get; set; } = null!;
}
