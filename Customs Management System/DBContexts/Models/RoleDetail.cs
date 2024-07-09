using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Customs_Management_System.DBContexts.Models;

public partial class RoleDetail
{
    [Key]
    public int RoleDetailsId { get; set; }

    public int RoleId { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? Address { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ContractNumber { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("RoleDetails")]
    public virtual Role Role { get; set; } = null!;
}
