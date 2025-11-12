using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace App_Indice_Glucemico.Server.Services;

/// <summary>
/// Implementación del servicio de envío de correos electrónicos usando MailKit
/// </summary>
public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _logger = logger;
        
        // Cargar configuración de SMTP
        _smtpSettings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>()
            ?? throw new InvalidOperationException("SmtpSettings no encontrado en la configuración.");
    }

    /// <summary>
    /// Envía un correo electrónico
    /// </summary>
    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        return await SendEmailAsync(new[] { to }, subject, body, isHtml);
    }

    /// <summary>
    /// Envía un correo electrónico a múltiples destinatarios
    /// </summary>
    public async Task<bool> SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = true)
    {
        try
        {
            var message = new MimeMessage();
            
            // Remitente
            message.From.Add(new MailboxAddress(_smtpSettings.Titulo, _smtpSettings.UserName));
            
            // Destinatarios
            foreach (var email in to)
            {
                message.To.Add(MailboxAddress.Parse(email));
            }
            
            // Asunto
            message.Subject = subject;
            
            // Cuerpo del mensaje
            var bodyBuilder = new BodyBuilder();
            if (isHtml)
            {
                bodyBuilder.HtmlBody = body;
            }
            else
            {
                bodyBuilder.TextBody = body;
            }
            
            message.Body = bodyBuilder.ToMessageBody();
            
            // Enviar el correo
            using var client = new SmtpClient();
            
            await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            _logger.LogInformation("Correo enviado exitosamente a {Recipients}", string.Join(", ", to));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo a {Recipients}", string.Join(", ", to));
            return false;
        }
    }
}

