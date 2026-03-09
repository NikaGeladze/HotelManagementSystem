using HMS.Application.Exceptions;
using HMS.Application.Interfaces.Services;
using HMS.Application.Interfaces.Wrappers;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

namespace HMS.Application.Services;

public class EmailService : IEmailService
{
    private readonly ISmtpClientWrapper _smtpClient;
    private readonly string _host;
    private readonly int _port;
    private readonly string _senderEmail;
    private readonly string _senderName;
    private readonly string _password;

    public EmailService(ISmtpClientWrapper smtpClient, IConfiguration configuration)
    {
        _smtpClient = smtpClient;
        _host = configuration.GetValue<string>("EmailSettings:Host");
        _port = configuration.GetValue<int>("EmailSettings:Port");
        _senderEmail = configuration.GetValue<string>("EmailSettings:SenderEmail");
        _senderName = configuration.GetValue<string>("EmailSettings:SenderName");
        _password = configuration.GetValue<string>("EmailSettings:Password");
    }

    public async Task SendAsync(string toEmail, string subject, string body)
    {
        if (!IsValidEmail(toEmail))
            throw new ValidationException([$"Invalid email address: {toEmail}"]);
        
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_senderName, _senderEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = body };

        await _smtpClient.ConnectAsync(_host, _port, SecureSocketOptions.Auto);
        await _smtpClient.AuthenticateAsync(_senderEmail, _password);
        await _smtpClient.SendAsync(email);
        await _smtpClient.DisconnectAsync(true);
    }
    
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = MailboxAddress.Parse(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}