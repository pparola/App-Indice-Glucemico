using Dapper;
using MySqlConnector;
using App_Indice_Glucemico.Shared;

namespace App_Indice_Glucemico.Server.Repositories;

/// <summary>
/// Implementación del repositorio de Alimentos usando Dapper
/// </summary>
public class AlimentoRepository : IAlimentoRepository
{
    private readonly string _connectionString;

    public AlimentoRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("app_indice_glucemico") 
            ?? throw new InvalidOperationException("Connection string 'app_indice_glucemico' not found.");
    }

    /// <summary>
    /// Obtiene un alimento por su ID
    /// </summary>
    public async Task<Alimento?> GetByIdAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        const string sql = @"
            SELECT Id, Nombre, IndiceGlucemico, CarbsPor100g, FuenteDatos 
            FROM Alimentos 
            WHERE Id = @Id";

        return await connection.QueryFirstOrDefaultAsync<Alimento>(sql, new { Id = id });
    }

    /// <summary>
    /// Obtiene todos los alimentos
    /// </summary>
    public async Task<IEnumerable<Alimento>> GetAllAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        const string sql = @"
            SELECT Id, Nombre, IndiceGlucemico, CarbsPor100g, FuenteDatos 
            FROM Alimentos 
            ORDER BY Nombre";

        return await connection.QueryAsync<Alimento>(sql);
    }

    /// <summary>
    /// Busca alimentos por nombre (búsqueda parcial)
    /// </summary>
    public async Task<IEnumerable<Alimento>> SearchByNameAsync(string nombre)
    {
        using var connection = new MySqlConnection(_connectionString);
        const string sql = @"
            SELECT Id, Nombre, IndiceGlucemico, CarbsPor100g, FuenteDatos 
            FROM Alimentos 
            WHERE Nombre LIKE @Nombre 
            ORDER BY Nombre";

        return await connection.QueryAsync<Alimento>(sql, new { Nombre = $"%{nombre}%" });
    }

    /// <summary>
    /// Agrega un nuevo alimento
    /// </summary>
    public async Task<Alimento> AddAsync(Alimento alimento)
    {
        using var connection = new MySqlConnection(_connectionString);
        const string sql = @"
            INSERT INTO Alimentos (Nombre, IndiceGlucemico, CarbsPor100g, FuenteDatos) 
            VALUES (@Nombre, @IndiceGlucemico, @CarbsPor100g, @FuenteDatos);
            SELECT LAST_INSERT_ID();";

        var id = await connection.ExecuteScalarAsync<int>(sql, alimento);
        alimento.Id = id;
        return alimento;
    }

    /// <summary>
    /// Actualiza un alimento existente
    /// </summary>
    public async Task<bool> UpdateAsync(Alimento alimento)
    {
        using var connection = new MySqlConnection(_connectionString);
        const string sql = @"
            UPDATE Alimentos 
            SET Nombre = @Nombre, 
                IndiceGlucemico = @IndiceGlucemico, 
                CarbsPor100g = @CarbsPor100g, 
                FuenteDatos = @FuenteDatos 
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, alimento);
        return rowsAffected > 0;
    }

    /// <summary>
    /// Elimina un alimento por su ID
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        const string sql = "DELETE FROM Alimentos WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }
}

