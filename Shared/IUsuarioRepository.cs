namespace App_Indice_Glucemico.Shared;

/// <summary>
/// Interfaz para el repositorio de Usuarios
/// </summary>
public interface IUsuarioRepository
{
    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    Task<Usuario?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene un usuario por su email
    /// </summary>
    Task<Usuario?> GetByEmailAsync(string email);

    /// <summary>
    /// Obtiene todos los usuarios
    /// </summary>
    Task<IEnumerable<Usuario>> GetAllAsync();

    /// <summary>
    /// Verifica si existe un usuario con el email especificado
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email);

    /// <summary>
    /// Agrega un nuevo usuario
    /// </summary>
    Task<Usuario> AddAsync(Usuario usuario);

    /// <summary>
    /// Actualiza un usuario existente
    /// </summary>
    Task<bool> UpdateAsync(Usuario usuario);

    /// <summary>
    /// Elimina un usuario por su ID
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Autentica un usuario por email y contraseña
    /// </summary>
    Task<Usuario?> AuthenticateAsync(string email, string password);
}

