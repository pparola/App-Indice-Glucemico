using Microsoft.AspNetCore.Mvc;
using App_Indice_Glucemico.Server.Services;

namespace App_Indice_Glucemico.Server.Controllers;

/// <summary>
/// Controller para gestionar el envío de correos electrónicos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(IEmailService emailService, ILogger<EmailController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Envía un correo electrónico
    /// </summary>
    /// <param name="request">Datos del correo a enviar</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("La solicitud no puede ser nula");
            }

            if (string.IsNullOrWhiteSpace(request.To))
            {
                return BadRequest("El destinatario es requerido");
            }

            if (string.IsNullOrWhiteSpace(request.Subject))
            {
                return BadRequest("El asunto es requerido");
            }

            if (string.IsNullOrWhiteSpace(request.Body))
            {
                return BadRequest("El cuerpo del mensaje es requerido");
            }

            // Validar formato de email básico
            if (!request.To.Contains("@") || !request.To.Contains("."))
            {
                return BadRequest("El formato del email del destinatario no es válido");
            }

            var resultado = await _emailService.SendEmailAsync(
                request.To,
                request.Subject,
                request.Body,
                request.IsHtml);

            if (resultado)
            {
                return Ok(new { message = "Correo enviado exitosamente", to = request.To });
            }
            else
            {
                return StatusCode(500, "Error al enviar el correo");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo");
            return StatusCode(500, "Error interno del servidor al enviar el correo");
        }
    }

    /// <summary>
    /// Envía un correo electrónico a múltiples destinatarios
    /// </summary>
    /// <param name="request">Datos del correo a enviar</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPost("send-multiple")]
    public async Task<IActionResult> SendEmailToMultiple([FromBody] SendEmailMultipleRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("La solicitud no puede ser nula");
            }

            if (request.To == null || !request.To.Any())
            {
                return BadRequest("Se requiere al menos un destinatario");
            }

            if (string.IsNullOrWhiteSpace(request.Subject))
            {
                return BadRequest("El asunto es requerido");
            }

            if (string.IsNullOrWhiteSpace(request.Body))
            {
                return BadRequest("El cuerpo del mensaje es requerido");
            }

            // Validar formato de emails
            foreach (var email in request.To)
            {
                if (!email.Contains("@") || !email.Contains("."))
                {
                    return BadRequest($"El formato del email '{email}' no es válido");
                }
            }

            var resultado = await _emailService.SendEmailAsync(
                request.To,
                request.Subject,
                request.Body,
                request.IsHtml);

            if (resultado)
            {
                return Ok(new { message = "Correo enviado exitosamente", to = request.To });
            }
            else
            {
                return StatusCode(500, "Error al enviar el correo");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo a múltiples destinatarios");
            return StatusCode(500, "Error interno del servidor al enviar el correo");
        }
    }
}

/// <summary>
/// Modelo para la solicitud de envío de correo a un destinatario
/// </summary>
public class SendEmailRequest
{
    /// <summary>
    /// Dirección de correo del destinatario
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Asunto del correo
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Cuerpo del correo
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el cuerpo es HTML (por defecto true)
    /// </summary>
    public bool IsHtml { get; set; } = true;
}

/// <summary>
/// Modelo para la solicitud de envío de correo a múltiples destinatarios
/// </summary>
public class SendEmailMultipleRequest
{
    /// <summary>
    /// Lista de direcciones de correo de los destinatarios
    /// </summary>
    public IEnumerable<string> To { get; set; } = new List<string>();

    /// <summary>
    /// Asunto del correo
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Cuerpo del correo
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el cuerpo es HTML (por defecto true)
    /// </summary>
    public bool IsHtml { get; set; } = true;
}

