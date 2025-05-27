using System;
using System.Collections.Generic;

namespace SpamShieldSystem.Models;

public partial class EmailExplanation
{
    public int ExplanationId { get; set; }

    public int EmailId { get; set; }

    public string PredictedLabel { get; set; } = null!;

    public string? Probabilities { get; set; }

    public string? KeyWords { get; set; }

    public string? ExplanationMessage { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Email Email { get; set; } = null!;
}
