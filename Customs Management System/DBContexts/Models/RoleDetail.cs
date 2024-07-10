using System;
using System.Collections.Generic;

namespace Customs_Management_System.DBContexts.Models;

public partial class RoleDetail
{
    public int RoleDetailsId { get; set; }

    public int RoleId { get; set; }

    public string? Address { get; set; }

    public string? ContractNumber { get; set; }

    public virtual Role Role { get; set; } = null!;
}
