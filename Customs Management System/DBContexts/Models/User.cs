using System;
using System.Collections.Generic;

namespace Customs_Management_System.DBContexts.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public int UserRoleId { get; set; }

    public string Password { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string Email { get; set; } = null!;

    public DateTime CreateAt { get; set; }

    public bool IsActive { get; set; }

    public int? LoginCount { get; set; }

    public virtual ICollection<Declaration> Declarations { get; set; } = new List<Declaration>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual Role UserRole { get; set; } = null!;
}
