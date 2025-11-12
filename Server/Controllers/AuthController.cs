using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using App_Indice_Glucemico.Shared;

namespace App_Indice_Glucemico.Server.Controllers;

/// <summary>
/// Controller para gestionar la autenticación
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUsuarioRepository usuarioRepository, ILogger<AuthController> logger)
    {
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    /// <summary>
    /// Inicia sesión de un usuario
    /// </summary>
    /// <param name="request">Credenciales de login</param>
    /// <returns>Resultado de la autenticación</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email y contraseña son requeridos" });
            }

            var usuario = await _usuarioRepository.AuthenticateAsync(request.Email, request.Password);
            
            if (usuario == null)
            {
                return Unauthorized(new { message = "Email o contraseña incorrectos" });
            }

            // Crear las claims del usuario
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Mantener la sesión activa
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30) // Cookie válida por 30 días
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("Usuario {Email} inició sesión exitosamente", usuario.Email);

            return Ok(new
            {
                message = "Login exitoso",
                usuario = new
                {
                    usuario.Id,
                    usuario.Nombre,
                    usuario.Email,
                    usuario.FechaCreacion,
                    usuario.Activo
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al autenticar usuario");
            return StatusCode(500, new { message = "Error interno del servidor al autenticar" });
        }
    }

    /// <summary>
    /// Cierra la sesión del usuario
    /// </summary>
    /// <returns>Resultado de la operación</returns>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("Usuario cerró sesión");
            return Ok(new { message = "Logout exitoso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cerrar sesión");
            return StatusCode(500, new { message = "Error interno del servidor al cerrar sesión" });
        }
    }

    /// <summary>
    /// Obtiene el usuario actual autenticado
    /// </summary>
    /// <returns>Usuario autenticado</returns>
    [HttpGet("current-user")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Usuario no válido" });
            }

            var usuario = await _usuarioRepository.GetByIdAsync(userId);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            return Ok(new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.FechaCreacion,
                usuario.Activo
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario actual");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

