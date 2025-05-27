using Microsoft.EntityFrameworkCore;
using SpamShieldSystem.Data;
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

                var result = JsonSerializer.Deserialize<ClassificationResult>(responseString);
                Console.WriteLine($"Deserialized Label: {result?.Label}");

                email.Label = result!.Label;
                email.CreatedAt = DateTime.Now.AddHours(7);
                Console.WriteLine($"Email CreatedAt: {email.CreatedAt}");

                await SaveEmail(email);
                Console.WriteLine("Email saved successfully.");

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

        public async Task<IEnumerable<Email>> GetEmailsByLabel(string label)
        {
            return await _context.Emails.Where(e => e.Label == label).ToListAsync();
        }
    }

    public class ClassificationResult
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = null!;

        [JsonPropertyName("probabilities")]
        public double[] Probabilities { get; set; } = null!;
    }
}