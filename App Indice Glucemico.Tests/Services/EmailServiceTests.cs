using App_Indice_Glucemico.Server.Services;
using App_Indice_Glucemico.Tests.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace App_Indice_Glucemico.Tests.Services;

/// <summary>
/// Tests de integración para EmailService
/// Estos tests envían correos reales a través de SMTP
/// </summary>
public class EmailServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;

    public EmailServiceTests(DatabaseFixture fixture)
    {
        _configuration = fixture.Configuration;
        var logger = new Mock<ILogger<EmailService>>().Object;
        _emailService = new EmailService(_configuration, logger);
    }

    [Fact]
    public async Task SendEmailAsync_SendsTestEmail_ToParola()
    {
        // Arrange
        var destinatario = "p.parola@gmail.com";
        var asunto = "Test de Email - App Índice Glucémico";
        var cuerpo = @"
            <html>
                <body>
                    <h1>Correo de Prueba</h1>
                    <p>Este es un correo de prueba enviado desde el sistema de App Índice Glucémico.</p>
                    <p>Fecha y hora: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"</p>
                    <p>Si recibes este correo, significa que el sistema de envío de emails está funcionando correctamente.</p>
                    <hr>
                    <p><small>Este es un correo automático, por favor no responder.</small></p>
                </body>
            </html>";

        // Act
        var resultado = await _emailService.SendEmailAsync(destinatario, asunto, cuerpo, isHtml: true);

        // Assert
        Assert.True(resultado, "El correo debería haberse enviado exitosamente");
    }

    [Fact]
    public async Task SendEmailAsync_SendsPlainTextEmail_ToParola()
    {
        // Arrange
        var destinatario = "p.parola@gmail.com";
        var asunto = "Test de Email (Texto Plano) - App Índice Glucémico";
        var cuerpo = "Este es un correo de prueba en texto plano enviado desde el sistema de App Índice Glucémico.\n\n" +
                     "Fecha y hora: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n\n" +
                     "Si recibes este correo, significa que el sistema de envío de emails está funcionando correctamente.";

        // Act
        var resultado = await _emailService.SendEmailAsync(destinatario, asunto, cuerpo, isHtml: false);

        // Assert
        Assert.True(resultado, "El correo en texto plano debería haberse enviado exitosamente");
    }

    [Fact]
    public async Task SendEmailAsync_SendsToMultipleRecipients()
    {
        // Arrange
        var destinatarios = new[] { "p.parola@gmail.com" };
        var asunto = "Test de Email Múltiple - App Índice Glucémico";
        var cuerpo = @"
            <html>
                <body>
                    <h1>Correo de Prueba Múltiple</h1>
                    <p>Este correo fue enviado a múltiples destinatarios.</p>
                    <p>Fecha y hora: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"</p>
                </body>
            </html>";

        // Act
        var resultado = await _emailService.SendEmailAsync(destinatarios, asunto, cuerpo, isHtml: true);

        // Assert
        Assert.True(resultado, "El correo a múltiples destinatarios debería haberse enviado exitosamente");
    }
}

