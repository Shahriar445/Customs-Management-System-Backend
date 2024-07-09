using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Customs_Management_System.DBContexts.Models;

[Table("Monitoring")]
public partial class Monitoring
{
    [Key]
    public int MonitoringId { get; set; }

    public int DeclarationId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? MethodOfShipment { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? PortOfDeparture { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? PortOfDestination { get; set; }

    public DateOnly? DepartureDate { get; set; }

    public DateOnly? ArrivalDate { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    [ForeignKey("DeclarationId")]
    [InverseProperty("Monitorings")]
    public virtual Declaration Declaration { get; set; } = null!;
}
