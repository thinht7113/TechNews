namespace TechNews.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
        Task SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string htmlBody);
    }
}
