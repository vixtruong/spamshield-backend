using Microsoft.AspNetCore.Mvc;
using SpamShieldSystem.Interfaces;
using SpamShieldSystem.Models;
using SpamShieldSystem.DTOs;
using System.Text.Json;

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

        [HttpGet("{emailId}")]
        public async Task<IActionResult> GetEmailDetail(int emailId)
        {
            var emailDto = await _emailService.GetEmailDetail(emailId);

            if (emailDto == null)
            {
                return NotFound();
            }

            return Ok(emailDto);
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
                var classifiedEmailsDto = await _emailService.GetClassifiedEmailsDto(emails);
                IEnumerable<EmailDetailDto> filteredEmails = classifiedEmailsDto;
                if (!string.IsNullOrEmpty(filterLabel))
                {
                    filteredEmails = classifiedEmailsDto.Where(e => e.Label == filterLabel).ToList();
                }

                var response = new ClassifiedEmailResponseDto
                {
                    ClassifiedEmails = filteredEmails.ToList(),
                    TotalCount = classifiedEmailsDto.Count,
                    FilteredCount = filteredEmails.Count()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}