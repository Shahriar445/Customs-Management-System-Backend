using Customs_Management_System.DBContexts.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Customs_Management_System.DTOs
{
    public class MonitoringDto
    {
      

        public string? MethodOfShipment { get; set; }

       
        public string? PortOfDeparture { get; set; }

     
        public string? PortOfDestination { get; set; }

        public DateTime? DepartureDate { get; set; }

        public DateTime? ArrivalDate { get; set; }

        public string? Status { get; set; }

        
       public ProductDto Product { get; set; } = null!;

        public ShipmentDto Shipment { get; set; } = null!;

      
        public virtual DeclarationDto Declaration { get; set; } = null!;
    }
}

