using Customs_Management_System.DBContexts.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Customs_Management_System.DTOs
{
    public class UserDto
    {
       
        public string UserName { get; set; } = null!;

        public int UserRoleId { get; set; }

     
        public string Password { get; set; } = null!;

        public DateTime CreateDate { get; set; }

       
        public string Email { get; set; } = null!;

        
        public DateTime CreateAt { get; set; }

   
        public virtual ICollection<DeclarationDto> Declarations { get; set; } = new List<DeclarationDto>();

      
        public virtual ICollection<PaymentDto> Payments { get; set; } = new List<PaymentDto>();

      
        public virtual ICollection<ReportDto> Reports { get; set; } = new List<ReportDto>();

        public virtual RoleDto UserRole { get; set; } = null!;


    }
}
