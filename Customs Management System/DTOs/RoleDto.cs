using Customs_Management_System.DBContexts.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Customs_Management_System.DTOs
{
    public class RoleDto
    {
   
        public int RoleId { get; set; }

     
        public string RoleName { get; set; } = null!;

      
        public virtual ICollection<RoleDetail> RoleDetails { get; set; } = new List<RoleDetail>();

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
