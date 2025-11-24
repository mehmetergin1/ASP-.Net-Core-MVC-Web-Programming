using System.Net;
using System.Net.Mail;

namespace CivicRequestPortal.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        try
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderPassword = _configuration["EmailSettings:SenderPassword"];

            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail))
            {
                _logger.LogWarning("Email settings not configured. Skipping email send.");
                return;
            }

            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail!),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {toEmail}");
            // Don't throw - email failures shouldn't break the application
        }
    }

    public async Task SendRequestSubmittedEmailAsync(string toEmail, string requestNumber, string title)
    {
        var subject = $"Yeni Şikayet Kaydı: {requestNumber}";
        var body = $@"
            <html>
            <body>
                <h2>Şikayetiniz Başarıyla Kaydedildi</h2>
                <p>Sayın Vatandaş,</p>
                <p>Şikayetiniz sisteme kaydedilmiştir.</p>
                <p><strong>Şikayet Numarası:</strong> {requestNumber}</p>
                <p><strong>Başlık:</strong> {title}</p>
                <p>Şikayetinizin durumunu takip etmek için portalımızı ziyaret edebilirsiniz.</p>
                <p>Teşekkürler,<br/>Belediye Yönetimi</p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendStatusUpdateEmailAsync(string toEmail, string requestNumber, string title, string newStatus)
    {
        var subject = $"Şikayet Durum Güncellemesi: {requestNumber}";
        var body = $@"
            <html>
            <body>
                <h2>Şikayet Durum Güncellemesi</h2>
                <p>Sayın Vatandaş,</p>
                <p>Şikayetinizin durumu güncellenmiştir.</p>
                <p><strong>Şikayet Numarası:</strong> {requestNumber}</p>
                <p><strong>Başlık:</strong> {title}</p>
                <p><strong>Yeni Durum:</strong> {newStatus}</p>
                <p>Detaylı bilgi için portalımızı ziyaret edebilirsiniz.</p>
                <p>Teşekkürler,<br/>Belediye Yönetimi</p>
            </body>
            </html>";

        await SendEmailAsync(toEmail, subject, body);
    }
}

