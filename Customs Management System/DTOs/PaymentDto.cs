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

        public DateTime Date { get; set; }

        public string Status { get; set; } = null!;

        public int? ProductId { get; set; }

        public virtual DeclarationDto Declaration { get; set; } = null!;

        public virtual ProductDto? Product { get; set; }

  
        public virtual UserDto User { get; set; } = null!;
    }
}
