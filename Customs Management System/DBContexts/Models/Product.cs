using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Customs_Management_System.DBContexts.Models;

public partial class Product
{
    [Key]
    public int ProductId { get; set; }

    public int DeclarationId { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string ProductName { get; set; } = null!;

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Weight { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? CountryOfOrigin { get; set; }

    [Column("HSCode")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Hscode { get; set; }

    [ForeignKey("DeclarationId")]
    [InverseProperty("Products")]
    public virtual Declaration Declaration { get; set; } = null!;

    [InverseProperty("Product")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
