namespace App_Indice_Glucemico.Shared;

/// <summary>
/// Representa un usuario del sistema
/// </summary>
public class Usuario
{
    /// <summary>
    /// Identificador único del usuario (Primary Key)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Email del usuario (único)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario (debería estar hasheada en producción)
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de creación del usuario
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Indica si el usuario está activo
    /// </summary>
    public bool Activo { get; set; } = true;
}

