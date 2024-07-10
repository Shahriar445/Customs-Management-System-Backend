using System;
using System.Collections.Generic;

namespace Customs_Management_System.DBContexts.Models;

public partial class Report
{
    public int ReportId { get; set; }

    public int UserId { get; set; }

    public string? ReportType { get; set; }

    public string? Content { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual User User { get; set; } = null!;
}
