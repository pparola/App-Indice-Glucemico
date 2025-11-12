using Microsoft.AspNetCore.Mvc;
using App_Indice_Glucemico.Shared;

namespace App_Indice_Glucemico.Server.Controllers;

/// <summary>
/// Controller para gestionar usuarios
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(IUsuarioRepository usuarioRepository, ILogger<UsuariosController> logger)
    {
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los usuarios
    /// </summary>
    /// <returns>Lista de usuarios</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Usuario>>> GetAll()
    {
        try
        {
            var usuarios = await _usuarioRepository.GetAllAsync();
            // No retornar las contraseñas por seguridad
            var usuariosSinPassword = usuarios.Select(u => new
            {
                u.Id,
                u.Nombre,
                u.Email,
                u.FechaCreacion,
                u.Activo
            });
            return Ok(usuariosSinPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los usuarios");
            return StatusCode(500, "Error interno del servidor al obtener los usuarios");
        }
    }

    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <returns>Usuario encontrado (sin contraseña)</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Usuario>> GetById(int id)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            
            if (usuario == null)
            {
                return NotFound($"No se encontró el usuario con ID {id}");
            }

            // No retornar la contraseña
            var usuarioSinPassword = new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.FechaCreacion,
                usuario.Activo
            };

            return Ok(usuarioSinPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el usuario con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor al obtener el usuario");
        }
    }

    /// <summary>
    /// Obtiene un usuario por su email
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <returns>Usuario encontrado (sin contraseña)</returns>
    [HttpGet("email/{email}")]
    public async Task<ActionResult<Usuario>> GetByEmail(string email)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByEmailAsync(email);
            
            if (usuario == null)
            {
                return NotFound($"No se encontró el usuario con email {email}");
            }

            // No retornar la contraseña
            var usuarioSinPassword = new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.FechaCreacion,
                usuario.Activo
            };

            return Ok(usuarioSinPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el usuario con email {Email}", email);
            return StatusCode(500, "Error interno del servidor al obtener el usuario");
        }
    }

    /// <summary>
    /// Crea un nuevo usuario
    /// </summary>
    /// <param name="usuario">Datos del usuario a crear</param>
    /// <returns>Usuario creado (sin contraseña)</returns>
    [HttpPost]
    public async Task<ActionResult<Usuario>> Create([FromBody] Usuario usuario)
    {
        try
        {
            if (usuario == null)
            {
                return BadRequest("El usuario no puede ser nulo");
            }

            if (string.IsNullOrWhiteSpace(usuario.Nombre))
            {
                return BadRequest("El nombre del usuario es requerido");
            }

            if (string.IsNullOrWhiteSpace(usuario.Email))
            {
                return BadRequest("El email del usuario es requerido");
            }

            // Validar formato de email básico
            if (!usuario.Email.Contains("@") || !usuario.Email.Contains("."))
            {
                return BadRequest("El formato del email no es válido");
            }

            if (string.IsNullOrWhiteSpace(usuario.Password))
            {
                return BadRequest("La contraseña es requerida");
            }

            if (usuario.Password.Length < 6)
            {
                return BadRequest("La contraseña debe tener al menos 6 caracteres");
            }

            // Verificar si el email ya existe
            var existe = await _usuarioRepository.ExistsByEmailAsync(usuario.Email);
            if (existe)
            {
                return Conflict($"Ya existe un usuario con el email {usuario.Email}");
            }

            // Establecer fecha de creación si no se proporciona
            if (usuario.FechaCreacion == default)
            {
                usuario.FechaCreacion = DateTime.Now;
            }

            var usuarioCreado = await _usuarioRepository.AddAsync(usuario);
            
            // No retornar la contraseña
            var usuarioSinPassword = new
            {
                usuarioCreado.Id,
                usuarioCreado.Nombre,
                usuarioCreado.Email,
                usuarioCreado.FechaCreacion,
                usuarioCreado.Activo
            };

            return CreatedAtAction(nameof(GetById), new { id = usuarioCreado.Id }, usuarioSinPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el usuario");
            return StatusCode(500, "Error interno del servidor al crear el usuario");
        }
    }

    /// <summary>
    /// Actualiza un usuario existente
    /// </summary>
    /// <param name="id">ID del usuario a actualizar</param>
    /// <param name="usuario">Datos actualizados del usuario</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Usuario usuario)
    {
        try
        {
            if (usuario == null)
            {
                return BadRequest("El usuario no puede ser nulo");
            }

            if (id != usuario.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del usuario");
            }

            if (string.IsNullOrWhiteSpace(usuario.Nombre))
            {
                return BadRequest("El nombre del usuario es requerido");
            }

            if (string.IsNullOrWhiteSpace(usuario.Email))
            {
                return BadRequest("El email del usuario es requerido");
            }

            // Validar formato de email básico
            if (!usuario.Email.Contains("@") || !usuario.Email.Contains("."))
            {
                return BadRequest("El formato del email no es válido");
            }

            // Verificar que el usuario existe
            var usuarioExistente = await _usuarioRepository.GetByIdAsync(id);
            if (usuarioExistente == null)
            {
                return NotFound($"No se encontró el usuario con ID {id}");
            }

            // Verificar si el email ya está en uso por otro usuario
            var usuarioConEmail = await _usuarioRepository.GetByEmailAsync(usuario.Email);
            if (usuarioConEmail != null && usuarioConEmail.Id != id)
            {
                return Conflict($"El email {usuario.Email} ya está en uso por otro usuario");
            }

            // Si no se proporciona contraseña, mantener la actual
            if (string.IsNullOrWhiteSpace(usuario.Password))
            {
                usuario.Password = usuarioExistente.Password;
            }
            else if (usuario.Password.Length < 6)
            {
                return BadRequest("La contraseña debe tener al menos 6 caracteres");
            }

            var resultado = await _usuarioRepository.UpdateAsync(usuario);
            
            if (!resultado)
            {
                return StatusCode(500, "Error al actualizar el usuario");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el usuario con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor al actualizar el usuario");
        }
    }

    /// <summary>
    /// Elimina un usuario por su ID
    /// </summary>
    /// <param name="id">ID del usuario a eliminar</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            // Verificar que el usuario existe
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
            {
                return NotFound($"No se encontró el usuario con ID {id}");
            }

            var resultado = await _usuarioRepository.DeleteAsync(id);
            
            if (!resultado)
            {
                return StatusCode(500, "Error al eliminar el usuario");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el usuario con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor al eliminar el usuario");
        }
    }

    /// <summary>
    /// Autentica un usuario por email y contraseña
    /// </summary>
    /// <param name="request">Credenciales de autenticación</param>
    /// <returns>Usuario autenticado (sin contraseña)</returns>
    [HttpPost("authenticate")]
    public async Task<ActionResult<Usuario>> Authenticate([FromBody] LoginRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email y contraseña son requeridos");
            }

            var usuario = await _usuarioRepository.AuthenticateAsync(request.Email, request.Password);
            
            if (usuario == null)
            {
                return Unauthorized("Email o contraseña incorrectos");
            }

            // No retornar la contraseña
            var usuarioSinPassword = new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.FechaCreacion,
                usuario.Activo
            };

            return Ok(usuarioSinPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al autenticar el usuario");
            return StatusCode(500, "Error interno del servidor al autenticar el usuario");
        }
    }
}

/// <summary>
/// Modelo para la solicitud de autenticación
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

