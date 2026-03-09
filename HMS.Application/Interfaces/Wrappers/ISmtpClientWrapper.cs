using MailKit.Security;
using MimeKit;

namespace HMS.Application.Interfaces.Wrappers;

public interface ISmtpClientWrapper : IDisposable
{
    Task ConnectAsync(string host, int port, SecureSocketOptions options);
    Task AuthenticateAsync(string email, string password);
    Task SendAsync(MimeMessage message);
    Task DisconnectAsync(bool quit);
}