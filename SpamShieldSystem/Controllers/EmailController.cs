using Microsoft.AspNetCore.Mvc;
using SpamShieldSystem.Interfaces;
using SpamShieldSystem.Models;
using SpamShieldSystem.Services;

namespace SpamShieldSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllEmails()
        {
            var emails = await _emailService.GetAllEmails();

            return Ok(emails);
        }

        [HttpPost("classify-list")]
        public async Task<IActionResult> ClassifyEmailList([FromBody] List<Email> emails, [FromQuery] string? filterLabel = null)
        {
            if (emails.Count == 0 || !emails.Any())
            {
                return BadRequest(new { Error = "Email list cannot be null or empty." });
            }

            try
            {
                // Phân loại tất cả email
                var classifiedEmails = await _emailService.ClassifyEmails(emails);

                // Lọc email theo nhãn nếu có filterLabel
                IEnumerable<Email> filteredEmails = classifiedEmails;
                if (!string.IsNullOrEmpty(filterLabel))
                {
                    filteredEmails = classifiedEmails.Where(e => e.Label == filterLabel).ToList();
                }

                return Ok(new
                {
                    ClassifiedEmails = filteredEmails,
                    TotalCount = classifiedEmails.Count,
                    FilteredCount = filteredEmails.Count()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}