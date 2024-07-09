using Customs_Management_System.DBContexts.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Customs_Management_System.DTOs
{
    public class PaymentDto
    {

       
        public int UserId { get; set; }

        public int DeclarationId { get; set; }

        public decimal Amount { get; set; }

        public DateOnly Date { get; set; }

        public string Status { get; set; } = null!;

        public int? ProductId { get; set; }

        public virtual Declaration Declaration { get; set; } = null!;

        public virtual Product? Product { get; set; }

  
        public virtual User User { get; set; } = null!;
    }
}
