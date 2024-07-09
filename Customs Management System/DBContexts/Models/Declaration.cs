using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Customs_Management_System.DBContexts.Models;

public partial class Declaration
{
    [Key]
    public int DeclarationId { get; set; }

    public int UserId { get; set; }

    public DateOnly DeclarationDate { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    [InverseProperty("Declaration")]
    public virtual ICollection<Monitoring> Monitorings { get; set; } = new List<Monitoring>();

    [InverseProperty("Declaration")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("Declaration")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [InverseProperty("Declaration")]
    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

    [ForeignKey("UserId")]
    [InverseProperty("Declarations")]
    public virtual User User { get; set; } = null!;
}
