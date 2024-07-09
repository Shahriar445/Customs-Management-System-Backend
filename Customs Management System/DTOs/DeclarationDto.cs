using Customs_Management_System.DBContexts.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Customs_Management_System.DTOs
{
    public class DeclarationDto
    {

        public int UserId { get; set; }

        public DateOnly DeclarationDate { get; set; }

        public string Status { get; set; } = null!;

        public virtual ICollection<Monitoring> Monitorings { get; set; } = new List<Monitoring>();

        
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        
        public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

        public virtual User User { get; set; } = null!;

    }
}
