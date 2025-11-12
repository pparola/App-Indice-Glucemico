using Dapper;
using MySqlConnector;
using App_Indice_Glucemico.Shared;

namespace App_Indice_Glucemico.Server.Repositories;

/// <summary>
/// Implementación del repositorio de Usuarios usando Dapper
/// </summary>
public class UsuarioRepository : IUsuarioRepository
{
    private readonly string _connectionString;

    public UsuarioRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("app_indice_glucemico") 
            ?? throw new InvalidOperationException("Connection string 'app_indice_glucemico' not found.");
    }

    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    public async Task<Usuario?> GetByIdAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        const string sql = @"
            SELECT Id, Nombre, Email, Password, FechaCreacion, Activo 
            FROM Usuarios 
            WHERE Id = @Id";

        return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene un usuario por su email
    /// </summary>
    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        const string sql = @"
            SELECT Id, Nombre, Email, Password, FechaCreacion, Activo 
            FROM Usuarios 
            WHERE Email = @Email";

        return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Email = email });
    }

    /// <summary>
    /// Obtiene todos los usuarios
    /// </summary>
    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        const string sql = @"
            SELECT Id, Nombre, Email, Password, FechaCreacion, Activo 
            FROM Usuarios 
            ORDER BY Nombre";

        return await connection.QueryAsync<Usuario>(sql);
    }

    /// <summary>
    /// Verifica si existe un usuario con el email especificado
    /// </summary>
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        const string sql = "SELECT COUNT(1) FROM Usuarios WHERE Email = @Email";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
        return count > 0;
    }

    /// <summary>
    /// Agrega un nuevo usuario
    /// </summary>
    public async Task<Usuario> AddAsync(Usuario usuario)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        const string sql = @"
            INSERT INTO Usuarios (Nombre, Email, Password, FechaCreacion, Activo) 
            VALUES (@Nombre, @Email, @Password, @FechaCreacion, @Activo);
            SELECT LAST_INSERT_ID();";

        // Establecer fecha de creación si no se proporciona
        if (usuario.FechaCreacion == default)
        {
            usuario.FechaCreacion = DateTime.Now;
        }

        var id = await connection.ExecuteScalarAsync<long>(sql, new
        {
            usuario.Nombre,
            usuario.Email,
            usuario.Password,
            usuario.FechaCreacion,
            Activo = usuario.Activo ? 1 : 0
        });

        usuario.Id = (int)id;
        return usuario;
    }

    /// <summary>
    /// Actualiza un usuario existente
    /// </summary>
    public async Task<bool> UpdateAsync(Usuario usuario)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        const string sql = @"
            UPDATE Usuarios 
            SET Nombre = @Nombre, 
                Email = @Email, 
                Password = @Password,
                Activo = @Activo
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            usuario.Id,
            usuario.Nombre,
            usuario.Email,
            usuario.Password,
            Activo = usuario.Activo ? 1 : 0
        });
        
        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina un usuario por su ID
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        const string sql = "DELETE FROM Usuarios WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    /// <summary>
    /// Autentica un usuario por email y contraseña
    /// </summary>
    public async Task<Usuario?> AuthenticateAsync(string email, string password)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        const string sql = @"
            SELECT Id, Nombre, Email, Password, FechaCreacion, Activo 
            FROM Usuarios 
            WHERE Email = @Email AND Password = @Password AND Activo = 1";

        return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new 
        { 
            Email = email, 
            Password = password 
        });
    }
}

