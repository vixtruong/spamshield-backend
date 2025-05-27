using Microsoft.EntityFrameworkCore;
using SpamShieldSystem.Data;
using SpamShieldSystem.DTOs;
using SpamShieldSystem.Interfaces;
using SpamShieldSystem.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpamShieldSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly SpamShieldContext _context;
        private readonly HttpClient _httpClient;

        public EmailService(SpamShieldContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://127.0.0.1:8000"); // Địa chỉ API FastAPI
        }

        public async Task SaveEmail(Email email)
        {
            _context.Add(email);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Email>> GetAllEmails()
        {
            return await _context.Emails.ToListAsync();
        }

        public async Task<string> ClassifyEmail(Email email)
        {
            if (string.IsNullOrEmpty(email.Content))
            {
                throw new ArgumentException("Email content cannot be null or empty.");
            }

            var requestBody = new { content = email.Content };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await _httpClient.PostAsync("/predict", jsonContent);
                Console.WriteLine($"Status Code: {response.StatusCode}");

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Raw Response: {responseString}");

                var result = JsonSerializer.Deserialize<ClassificationResultDto>(responseString);
                Console.WriteLine($"Deserialized Label: {result?.Label}");

                email.Label = result!.Label;
                email.CreatedAt = DateTime.Now.AddHours(7);
                Console.WriteLine($"Email CreatedAt: {email.CreatedAt}");

                await SaveEmail(email);
                Console.WriteLine("Email saved successfully.");

                int emailId = email.EmailId; // Lấy EmailId sau khi lưu

                var emailExplanation = new EmailExplanation
                {
                    EmailId = emailId,
                    PredictedLabel = result.Label,
                    Probabilities = JsonSerializer.Serialize(new { ham = result.Probabilities[0], spam = result.Probabilities[1] }),
                    KeyWords = JsonSerializer.Serialize(result.Explanation.KeyWords),
                    ExplanationMessage = result.Explanation.Message,
                    CreatedAt = DateTime.Now.AddHours(7)
                };

                _context.EmailExplanations.Add(emailExplanation);
                await _context.SaveChangesAsync();
                Console.WriteLine("Explanation saved successfully.");

                return result.Label;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to call classification API.", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to parse API response.", ex);
            }
        }

        public async Task<List<Email>> ClassifyEmails(List<Email> emails)
        {
            var classifiedEmails = new List<Email>();

            foreach (var email in emails)
            {
                try
                {
                    await ClassifyEmail(email);
                    classifiedEmails.Add(email);
                }
                catch (Exception ex)
                {
                    email.Label = "error";
                    email.CreatedAt = DateTime.Now;
                    await SaveEmail(email);
                    classifiedEmails.Add(email);
                }
            }

            return classifiedEmails;
        }

        public async Task<EmailDetailDto> GetEmailDetail(int emailId)
        {
            var email = await _context.Emails
                .Include(e => e.EmailExplanations)
                .FirstOrDefaultAsync(e => e.EmailId == emailId);

            if (email == null)
            {
                return null;
            }

            var dto = new EmailDetailDto
            {
                EmailId = email.EmailId,
                Sender = email.Sender,
                Subject = email.Subject,
                Content = email.Content,
                Label = email.Label,
                EmailDate = email.EmailDate,
                CreatedAt = email.CreatedAt,
                EmailExplanations = email.EmailExplanations.Select(ex => new EmailExplanationDto
                {
                    ExplanationId = ex.ExplanationId,
                    EmailId = ex.EmailId,
                    PredictedLabel = ex.PredictedLabel,
                    Probabilities = JsonSerializer.Deserialize<Dictionary<string, double>>(ex.Probabilities),
                    KeyWords = JsonSerializer.Deserialize<List<KeyWordDto>>(ex.KeyWords),
                    ExplanationMessage = ex.ExplanationMessage,
                    CreatedAt = ex.CreatedAt
                }).ToList()
            };

            return dto;
        }

        public async Task<IEnumerable<Email>> GetEmailsByLabel(string label)
        {
            return await _context.Emails.Where(e => e.Label == label).ToListAsync();
        }

        // Thêm phương thức để ánh xạ danh sách Email sang DTO
        public async Task<List<EmailDetailDto>> GetClassifiedEmailsDto(List<Email> emails)
        {
            var classifiedEmails = await ClassifyEmails(emails);
            return classifiedEmails.Select(email => new EmailDetailDto
            {
                EmailId = email.EmailId,
                Sender = email.Sender,
                Subject = email.Subject,
                Content = email.Content,
                Label = email.Label,
                EmailDate = email.EmailDate,
                CreatedAt = email.CreatedAt,
                EmailExplanations = email.EmailExplanations.Select(ex => new EmailExplanationDto
                {
                    ExplanationId = ex.ExplanationId,
                    EmailId = ex.EmailId,
                    PredictedLabel = ex.PredictedLabel,
                    Probabilities = JsonSerializer.Deserialize<Dictionary<string, double>>(ex.Probabilities),
                    KeyWords = JsonSerializer.Deserialize<List<KeyWordDto>>(ex.KeyWords),
                    ExplanationMessage = ex.ExplanationMessage,
                    CreatedAt = ex.CreatedAt
                }).ToList()
            }).ToList();
        }
    }

    // DTO cho key_words
    public class KeyWordDtoInClass
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

    // DTO cho explanation
    public class ExplanationDto
    {
        [JsonPropertyName("key_words")]
        public List<KeyWordDtoInClass> KeyWords { get; set; } = null!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;
    }

    // DTO cho toàn bộ phản hồi từ API /predict
    public class ClassificationResultDto
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = null!;

        [JsonPropertyName("probabilities")]
        public double[] Probabilities { get; set; } = null!;

        [JsonPropertyName("explanation")]
        public ExplanationDto Explanation { get; set; } = null!;
    }
}