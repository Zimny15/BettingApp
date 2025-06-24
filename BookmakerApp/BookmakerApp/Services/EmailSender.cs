namespace BookmakerApp.Services;

using BookmakerApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
public class EmailSender : IEmailSender<ApplicationUser>
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        _logger.LogInformation("SendConfirmationLinkAsync called for {Email}", email);
        var subject = "Confirm your email";
        var message = $@"
            <div>
                <img src='https://localhost:7194/logo.jpeg' alt='Logo' style='max-width:400px;' />
                <p>Click the link to confirm your email: <a href='{confirmationLink}'>Confirm your email</a></p>
            </div>";
        return SendEmailAsync(email, subject, message);
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation("Attempting to send email to: {Email} with subject '{Subject}'", email, subject);
        try
        {
            _logger.LogInformation("Sending email to {Email} with subject '{Subject}'", email, subject);

            var smtpClient = new SmtpClient(_config["EmailSettings:SmtpHost"])
            {
                Port = int.Parse(_config["EmailSettings:SmtpPort"]),
                Credentials = new NetworkCredential(
                    _config["EmailSettings:Sender"],
                    _config["EmailSettings:Password"]
                ),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_config["EmailSettings:Sender"]),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mail.To.Add(email);

            return smtpClient.SendMailAsync(mail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
            throw;
        }
    }


    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var subject = "Your Password Reset Code";
        var message = $"Hello,<br/><br/>Here is your password reset code: <strong>{resetCode}</strong>.<br/><br/>" +
                      $"If you did not request this, please ignore this message.";
        return SendEmailAsync(email, subject, message);
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var subject = "Reset Your Password";
        var message = $"Hello,<br/><br/>You can reset your password by clicking the link below:<br/>" +
                      $"<a href='{resetLink}'>Reset Password</a><br/><br/>" +
                      $"If you did not request this, please ignore this message.";
        return SendEmailAsync(email, subject, message);
    }

}
