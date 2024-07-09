using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Customs_Management_System.DBContexts.Models;

public partial class User
{
    [Key]
    public int UserId { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string UserName { get; set; } = null!;

    public int UserRoleId { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string Password { get; set; } = null!;

    public DateOnly CreateDate { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreateAt { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Declaration> Declarations { get; set; } = new List<Declaration>();

    [InverseProperty("User")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("User")]
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    [ForeignKey("UserRoleId")]
    [InverseProperty("Users")]
    public virtual Role UserRole { get; set; } = null!;
}
