using System.Security.Claims;
using App_Indice_Glucemico.Shared;

namespace App_Indice_Glucemico.Server.Services;

/// <summary>
/// Servicio de autenticación
/// </summary>
public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUsuarioRepository _usuarioRepository;

    public AuthService(IHttpContextAccessor httpContextAccessor, IUsuarioRepository usuarioRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Obtiene el usuario actual autenticado
    /// </summary>
    public async Task<Usuario?> GetCurrentUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null || !httpContext.User.Identity?.IsAuthenticated == true)
        {
            return null;
        }

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return await _usuarioRepository.GetByIdAsync(userId);
    }

    /// <summary>
    /// Verifica si el usuario está autenticado
    /// </summary>
    public Task<bool> IsAuthenticatedAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(httpContext.User.Identity?.IsAuthenticated == true);
    }
}

