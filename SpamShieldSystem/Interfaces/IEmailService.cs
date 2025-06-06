﻿using SpamShieldSystem.DTOs;
using SpamShieldSystem.Models;

namespace SpamShieldSystem.Interfaces
{
    public interface IEmailService
    {
        Task SaveEmail(Email email);
        Task<IEnumerable<Email>> GetAllEmails();
        Task<string> ClassifyEmail(Email email);
        Task<List<Email>> ClassifyEmails(List<Email> emails);
        Task<IEnumerable<Email>> GetEmailsByLabel(string label);
        Task<EmailDetailDto> GetEmailDetail(int emailId);
        Task<List<EmailDetailDto>> GetClassifiedEmailsDto(List<Email> emails);
    }
}