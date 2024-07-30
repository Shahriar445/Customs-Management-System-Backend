using Customs_Management_System.DBContexts.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Customs_Management_System.DTOs
{
    public class MonitoringDto
    {


        public int DeclarationId { get; set; }
        
        public string? MethodOfShipment { get; set; }
        public string? PortOfDeparture { get; set; }
        public string? PortOfDestination { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public string? Status { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public double Weight { get; set; }
        public string? CountryOfOrigin { get; set; }
        public string? Hscode { get; set; }
    }
}

