namespace App_Indice_Glucemico.Server.Services;

/// <summary>
/// Configuración de SMTP para envío de correos electrónicos
/// </summary>
public class SmtpSettings
{
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

