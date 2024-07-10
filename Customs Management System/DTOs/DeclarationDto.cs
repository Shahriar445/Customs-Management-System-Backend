using Customs_Management_System.DBContexts.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Customs_Management_System.DTOs
{
    public class DeclarationDto
    {

        public int UserId { get; set; }

        public DateTime DeclarationDate { get; set; }

        public string Status { get; set; } = null!;



        // public virtual ICollection<ProductDto> Products { get; set; } = new List<ProductDto>();


        // public virtual ICollection<ShipmentDto> Shipments { get; set; } = new List<ShipmentDto>();
        public List<ProductDto> Products { get; set; }

        public List<ShipmentDto>Shipments { get; set; }
       
 

    }
}
