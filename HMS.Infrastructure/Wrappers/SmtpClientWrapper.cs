using HMS.Application.Interfaces.Wrappers;
using MailKit.Security;
using MimeKit;

namespace HMS.Infrastructure.Wrappers;

public class SmtpClientWrapper : ISmtpClientWrapper
{
    private readonly MailKit.Net.Smtp.SmtpClient _client = new();

    public async Task ConnectAsync(string host, int port, SecureSocketOptions options) =>
        await _client.ConnectAsync(host, port, options);

    public async Task AuthenticateAsync(string email, string password) =>
        await _client.AuthenticateAsync(email, password);

    public async Task SendAsync(MimeMessage message) =>
        await _client.SendAsync(message);

    public async Task DisconnectAsync(bool quit) =>
        await _client.DisconnectAsync(quit);

    public void Dispose() => _client.Dispose();
}