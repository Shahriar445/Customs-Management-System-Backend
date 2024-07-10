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

      
        public virtual ICollection<RoleDetailDto> RoleDetails { get; set; } = new List<RoleDetailDto>();

        public virtual ICollection<UserDto> Users { get; set; } = new List<UserDto>();
    }
}
