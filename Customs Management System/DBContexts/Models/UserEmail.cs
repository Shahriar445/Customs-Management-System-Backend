using System;
using System.Collections.Generic;

namespace Customs_Management_System.DBContexts.Models;

public partial class UserEmail
{
    public int EmailId { get; set; }

    public int? UserId { get; set; }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public DateTime? SentAt { get; set; }
}
