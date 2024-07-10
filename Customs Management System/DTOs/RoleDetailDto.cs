using Customs_Management_System.DBContexts.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Customs_Management_System.DTOs
{
    public class RoleDetailDto
    {

        public int RoleDetailsId { get; set; }

        public int RoleId { get; set; }

       
        public string? Address { get; set; }

        public string? ContractNumber { get; set; }

      
        public virtual RoleDto Role { get; set; } = null!;
    }
}
