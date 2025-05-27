using SpamShieldSystem.Services;
using System.Text.Json.Serialization;

namespace SpamShieldSystem.DTOs
{
    // DTO cho phản hồi của ClassifyEmailList
    public class ClassifiedEmailResponseDto
    {
        public List<EmailDetailDto> ClassifiedEmails { get; set; } = new List<EmailDetailDto>();
        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }
    }

    // Các DTO hiện có
    public class EmailDetailDto
    {
        public int EmailId { get; set; }
        public string Sender { get; set; } = null!;
        public string? Subject { get; set; }
        public string? Content { get; set; }
        public string? Label { get; set; }
        public DateTime EmailDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<EmailExplanationDto> EmailExplanations { get; set; } = new List<EmailExplanationDto>();
    }

    public class EmailExplanationDto
    {
        public int ExplanationId { get; set; }
        public int EmailId { get; set; }
        public string PredictedLabel { get; set; } = null!;
        public Dictionary<string, double> Probabilities { get; set; }
        public List<KeyWordDto>? KeyWords { get; set; }
        public string? ExplanationMessage { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class KeyWordDto
    {
        [JsonPropertyName("word")]
        public string Word { get; set; } = null!;

        [JsonPropertyName("tfidf_score")]
        public double TfidfScore { get; set; }

        [JsonPropertyName("spam_contribution")]
        public double SpamContribution { get; set; }

        [JsonPropertyName("ham_contribution")]
        public double HamContribution { get; set; }
    }
}