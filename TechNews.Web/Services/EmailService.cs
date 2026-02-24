using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using TechNews.Application.Interfaces;
using TechNews.Infrastructure.Data;

namespace TechNews.Web.Services
{
    public class EmailService : IEmailService
    {
        private readonly TechNewsDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(TechNewsDbContext context, IConfiguration config, ILogger<EmailService> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        private async Task<string> GetSettingAsync(string key, string fallbackConfigKey = "")
        {
            var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting != null && !string.IsNullOrEmpty(setting.Value))
                return setting.Value;

            if (!string.IsNullOrEmpty(fallbackConfigKey))
                return _config[fallbackConfigKey] ?? "";

            return "";
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                var smtpHost = await GetSettingAsync("SmtpHost", "Email:SmtpHost");
                var smtpPortStr = await GetSettingAsync("SmtpPort", "Email:SmtpPort");
                var smtpUser = await GetSettingAsync("SmtpUser", "Email:SmtpUser");
                var smtpPass = await GetSettingAsync("SmtpPass", "Email:SmtpPass");
                var fromName = await GetSettingAsync("SmtpFromName", "Email:FromName");

                if (string.IsNullOrEmpty(smtpHost)) smtpHost = "smtp.gmail.com";
                if (!int.TryParse(smtpPortStr, out var smtpPort)) smtpPort = 587;
                if (string.IsNullOrEmpty(fromName)) fromName = "TechNews";

                var fromEmail = smtpUser;

                if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                {
                    _logger.LogWarning("SMTP credentials not configured. Email not sent to {To}", to);
                    throw new InvalidOperationException("Chưa cấu hình SMTP. Vui lòng cập nhật trong Cấu hình → Email (Newsletter).");
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent to {To}", to);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }

        public async Task SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string htmlBody)
        {
            var recipientList = recipients.ToList();
            var failed = new List<string>();

            foreach (var email in recipientList)
            {
                try
                {
                    await SendEmailAsync(email, subject, htmlBody);
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send to {Email}", email);
                    failed.Add(email);
                }

                await Task.Delay(100);
            }

            if (failed.Any())
                _logger.LogWarning("Failed to send to {Count} recipients: {Emails}", failed.Count, string.Join(", ", failed));
        }
    }
}