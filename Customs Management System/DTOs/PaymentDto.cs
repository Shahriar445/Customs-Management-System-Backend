using Customs_Management_System.DBContexts.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Customs_Management_System.DTOs
{
    public class PaymentDto
    {

       public int PaymentId { get; set; }
        public int UserId { get; set; }

        public int DeclarationId { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public string Status { get; set; } = null!;

        public int? ProductId { get; set; }
        public string ProductName { get; set; } = null!;






    }
}
