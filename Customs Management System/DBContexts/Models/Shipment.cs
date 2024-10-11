using System;
using System.Collections.Generic;

namespace Customs_Management_System.DBContexts.Models;

public partial class Shipment
{
    public int ShipmentId { get; set; }

    public int DeclarationId { get; set; }

    public string? MethodOfShipment { get; set; }

    public string? PortOfDeparture { get; set; }

    public string? PortOfDestination { get; set; }

    public DateTime DepartureDate { get; set; }

    public DateTime ArrivalDate { get; set; }

    public string? Status { get; set; }

    public DateTime? CompletedDate { get; set; }

    public virtual Declaration Declaration { get; set; } = null!;
}
