using App_Indice_Glucemico.Shared;

namespace App_Indice_Glucemico.Server.Services;

/// <summary>
/// Interfaz para el servicio de autenticación
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Obtiene el usuario actual autenticado
    /// </summary>
    Task<Usuario?> GetCurrentUserAsync();

    /// <summary>
    /// Verifica si el usuario está autenticado
    /// </summary>
    Task<bool> IsAuthenticatedAsync();
}

