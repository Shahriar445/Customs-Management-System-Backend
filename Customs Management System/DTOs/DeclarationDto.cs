using Customs_Management_System.DBContexts.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Customs_Management_System.DTOs
{
    public class DeclarationDto
    {
        public int UserId { get; set; }
        public int DeclarationId { get; set; }
        public DateTime DeclarationDate { get; set; }
        public string Status { get; set; }
        public bool IsActive {  get; set; }
        public List<ProductDto> Products { get; set; }
        public List<ShipmentDto> Shipments { get; set; }

       
    }
}
