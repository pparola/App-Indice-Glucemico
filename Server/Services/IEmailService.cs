namespace App_Indice_Glucemico.Server.Services;

/// <summary>
/// Interfaz para el servicio de envío de correos electrónicos
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía un correo electrónico
    /// </summary>
    /// <param name="to">Dirección de correo del destinatario</param>
    /// <param name="subject">Asunto del correo</param>
    /// <param name="body">Cuerpo del correo (puede ser HTML)</param>
    /// <param name="isHtml">Indica si el cuerpo es HTML</param>
    /// <returns>True si el correo se envió correctamente</returns>
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);

    /// <summary>
    /// Envía un correo electrónico a múltiples destinatarios
    /// </summary>
    /// <param name="to">Lista de direcciones de correo de los destinatarios</param>
    /// <param name="subject">Asunto del correo</param>
    /// <param name="body">Cuerpo del correo (puede ser HTML)</param>
    /// <param name="isHtml">Indica si el cuerpo es HTML</param>
    /// <returns>True si el correo se envió correctamente</returns>
    Task<bool> SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = true);
}

