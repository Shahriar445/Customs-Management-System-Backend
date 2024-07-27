using System;
using System.Collections.Generic;

namespace Customs_Management_System.DBContexts.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<Declaration> Declarations { get; set; } = new List<Declaration>();

    public virtual ICollection<RoleDetail> RoleDetails { get; set; } = new List<RoleDetail>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
