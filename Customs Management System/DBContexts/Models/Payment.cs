using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Customs_Management_System.DBContexts.Models;

public partial class Payment
{
    [Key]
    public int PaymentId { get; set; }

    public int UserId { get; set; }

    public int DeclarationId { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Amount { get; set; }

    public DateOnly Date { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    public int? ProductId { get; set; }

    [ForeignKey("DeclarationId")]
    [InverseProperty("Payments")]
    public virtual Declaration Declaration { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("Payments")]
    public virtual Product? Product { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Payments")]
    public virtual User User { get; set; } = null!;
}
