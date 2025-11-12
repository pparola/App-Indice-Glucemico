using System.Net.Http.Json;
using System.Text.Json;

namespace App_Indice_Glucemico.Client.Services;

/// <summary>
/// Servicio de autenticación en el cliente
/// </summary>
public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService>? _logger;
    private UsuarioInfo? _currentUser;

    public AuthService(HttpClient httpClient, ILogger<AuthService>? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Evento que se dispara cuando el estado de autenticación cambia
    /// </summary>
    public event Action? OnAuthStateChanged;

    /// <summary>
    /// Usuario actual autenticado
    /// </summary>
    public UsuarioInfo? CurrentUser => _currentUser;

    /// <summary>
    /// Indica si el usuario está autenticado
    /// </summary>
    public bool IsAuthenticated => _currentUser != null;

    /// <summary>
    /// Inicia sesión
    /// </summary>
    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new
            {
                Email = email,
                Password = password
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.Usuario != null)
                {
                    _currentUser = result.Usuario;
                    OnAuthStateChanged?.Invoke();
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al iniciar sesión");
            return false;
        }
    }

    /// <summary>
    /// Cierra sesión
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            await _httpClient.PostAsync("/api/auth/logout", null);
            _currentUser = null;
            OnAuthStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al cerrar sesión");
        }
    }

    /// <summary>
    /// Obtiene el usuario actual
    /// </summary>
    public async Task<UsuarioInfo?> GetCurrentUserAsync()
    {
        try
        {
            if (_currentUser != null)
                return _currentUser;

            var response = await _httpClient.GetAsync("/api/auth/current-user");
            if (response.IsSuccessStatusCode)
            {
                _currentUser = await response.Content.ReadFromJsonAsync<UsuarioInfo>();
                OnAuthStateChanged?.Invoke();
                return _currentUser;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener usuario actual");
            return null;
        }
    }

    /// <summary>
    /// Registra un nuevo usuario
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(string nombre, string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/usuarios", new
            {
                Nombre = nombre,
                Email = email,
                Password = password,
                FechaCreacion = DateTime.Now,
                Activo = true
            });

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al registrar usuario");
            return (false, "Error al registrar usuario");
        }
    }
}

/// <summary>
/// Información del usuario
/// </summary>
public class UsuarioInfo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }
}

/// <summary>
/// Respuesta de login
/// </summary>
public class LoginResponse
{
    public string Message { get; set; } = string.Empty;
    public UsuarioInfo? Usuario { get; set; }
}

