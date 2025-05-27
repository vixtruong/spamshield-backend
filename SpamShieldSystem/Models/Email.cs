using System;
using System.Collections.Generic;

namespace SpamShieldSystem.Models;

public partial class Email
{
    public int EmailId { get; set; }

    public string Sender { get; set; } = null!;

    public string? Subject { get; set; }

    public string? Content { get; set; }

    public string? Label { get; set; }

    public DateTime EmailDate { get; set; }

    public DateTime? CreatedAt { get; set; }
}
