using Customs_Management_System.DBContexts.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Customs_Management_System.DTOs
{
    public class ProductDto
    {

      

        public int DeclarationId { get; set; }

   
        public string ProductName { get; set; } = null!;

        public int Quantity { get; set; }

    
        public decimal Weight { get; set; }

    
        public string? CountryOfOrigin { get; set; }

        public string? Hscode { get; set; }
        public string? Category { get; set; }
        public decimal? TotalPrice { get; set; }


    }
}

