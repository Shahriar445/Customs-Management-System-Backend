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

        public DateOnly CreateDate { get; set; }

       
        public string Email { get; set; } = null!;

        
        public DateTime CreateAt { get; set; }

   
        public virtual ICollection<Declaration> Declarations { get; set; } = new List<Declaration>();

      
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

      
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

        public virtual Role UserRole { get; set; } = null!;


    }
}
