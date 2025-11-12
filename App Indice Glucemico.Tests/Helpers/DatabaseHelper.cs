using Dapper;
using MySqlConnector;
using App_Indice_Glucemico.Tests.Helpers;

namespace App_Indice_Glucemico.Tests.Helpers;

/// <summary>
/// Clase helper para operaciones de base de datos en tests
/// </summary>
public static class DatabaseHelper
{
    private static string ConnectionString => TestConfiguration.GetConnectionString();

    /// <summary>
    /// Limpia todos los datos de las tablas de tests
    /// </summary>
    public static async Task CleanDatabaseAsync()
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        
        // Eliminar registros en orden (primero las dependencias)
        await connection.ExecuteAsync("DELETE FROM `RegistroComidas`");
        await connection.ExecuteAsync("DELETE FROM `Alimentos`");
    }

    /// <summary>
    /// Crea un alimento de prueba en la base de datos
    /// </summary>
    public static async Task<App_Indice_Glucemico.Shared.Alimento> CreateTestAlimentoAsync(
        string nombre = "Test Alimento",
        int indiceGlucemico = 50,
        decimal carbsPor100g = 20.0m,
        string? fuenteDatos = "TEST")
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        
        // Insertar el alimento
        const string insertSql = @"
            INSERT INTO `Alimentos` (`Nombre`, `IndiceGlucemico`, `CarbsPor100g`, `FuenteDatos`) 
            VALUES (@Nombre, @IndiceGlucemico, @CarbsPor100g, @FuenteDatos);";

        await connection.ExecuteAsync(insertSql, new
        {
            Nombre = nombre,
            IndiceGlucemico = indiceGlucemico,
            CarbsPor100g = carbsPor100g,
            FuenteDatos = fuenteDatos
        });

        // Obtener el ID del alimento insertado usando LAST_INSERT_ID en la misma conexión
        // Esto es crítico: LAST_INSERT_ID() solo funciona en la misma conexión
        var id = await connection.ExecuteScalarAsync<long>("SELECT LAST_INSERT_ID();");

        if (id <= 0)
        {
            throw new InvalidOperationException(
                $"No se pudo obtener el ID del alimento insertado '{nombre}'. LAST_INSERT_ID() retornó: {id}");
        }

        // Obtener el alimento completo usando el ID obtenido
        // Usar la misma conexión para asegurar consistencia
        var alimentoVerificado = await connection.QueryFirstOrDefaultAsync<App_Indice_Glucemico.Shared.Alimento>(
            "SELECT Id, Nombre, IndiceGlucemico, CarbsPor100g, FuenteDatos FROM `Alimentos` WHERE Id = @Id",
            new { Id = (int)id });

        if (alimentoVerificado == null)
        {
            // Si aún no se encuentra, intentar una vez más con un pequeño delay
            // Esto puede ser necesario en algunos casos de concurrencia
            await Task.Delay(100);
            alimentoVerificado = await connection.QueryFirstOrDefaultAsync<App_Indice_Glucemico.Shared.Alimento>(
                "SELECT Id, Nombre, IndiceGlucemico, CarbsPor100g, FuenteDatos FROM `Alimentos` WHERE Id = @Id",
                new { Id = (int)id });
        }

        if (alimentoVerificado == null)
        {
            throw new InvalidOperationException(
                $"No se pudo crear el alimento '{nombre}' en la base de datos. ID obtenido: {id}");
        }

        return alimentoVerificado;
    }

    /// <summary>
    /// Verifica si existe un alimento por ID
    /// </summary>
    public static async Task<bool> AlimentoExistsAsync(int id)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        
        const string sql = "SELECT COUNT(1) FROM `Alimentos` WHERE `Id` = @Id";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    /// <summary>
    /// Verifica si existe un registro por ID
    /// </summary>
    public static async Task<bool> RegistroExistsAsync(int id)
    {
        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        
        const string sql = "SELECT COUNT(1) FROM `RegistroComidas` WHERE `Id` = @Id";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }
}

