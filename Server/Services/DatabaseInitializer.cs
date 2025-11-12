using MySqlConnector;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace App_Indice_Glucemico.Server.Services;

/// <summary>
/// Servicio para inicializar la base de datos automáticamente si no existe
/// </summary>
public class DatabaseInitializer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IConfiguration configuration, ILogger<DatabaseInitializer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Inicializa la base de datos: crea la base de datos si no existe y crea las tablas necesarias
    /// </summary>
    public async Task InitializeAsync()
    {
        var connectionString = _configuration.GetConnectionString("app_indice_glucemico")
            ?? throw new InvalidOperationException("Connection string 'app_indice_glucemico' not found.");

        // Extraer información de la cadena de conexión
        var builder = new MySqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;
        builder.Database = null; // Conectar sin especificar la base de datos

        try
        {
            // Paso 1: Verificar y crear la base de datos si no existe
            using (var connection = new MySqlConnection(builder.ConnectionString))
            {
                await connection.OpenAsync();
                
                var createDatabaseSql = $@"
                    CREATE DATABASE IF NOT EXISTS `{databaseName}`
                    CHARACTER SET utf8mb4
                    COLLATE utf8mb4_unicode_ci;";

                await connection.ExecuteNonQueryAsync(createDatabaseSql);
                _logger.LogInformation("Base de datos '{DatabaseName}' verificada/creada correctamente", databaseName);
            }

            // Paso 2: Crear las tablas si no existen
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Crear tabla Alimentos
                var createAlimentosTable = @"
                    CREATE TABLE IF NOT EXISTS `Alimentos` (
                        `Id` INT NOT NULL AUTO_INCREMENT,
                        `Nombre` VARCHAR(255) NOT NULL,
                        `IndiceGlucemico` INT NOT NULL,
                        `CarbsPor100g` DECIMAL(10, 2) NOT NULL,
                        `FuenteDatos` VARCHAR(100) NULL,
                        PRIMARY KEY (`Id`),
                        INDEX `idx_nombre` (`Nombre`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

                await connection.ExecuteNonQueryAsync(createAlimentosTable);
                _logger.LogInformation("Tabla 'Alimentos' verificada/creada correctamente");

                // Crear tabla RegistroComidas
                var createRegistroComidasTable = @"
                    CREATE TABLE IF NOT EXISTS `RegistroComidas` (
                        `Id` INT NOT NULL AUTO_INCREMENT,
                        `AlimentoId` INT NOT NULL,
                        `FechaHora` DATETIME NOT NULL,
                        `GramosConsumidos` DECIMAL(10, 2) NOT NULL,
                        `TipoComida` INT NOT NULL,
                        `CargaGlucemicaCalculada` DECIMAL(10, 2) NULL,
                        PRIMARY KEY (`Id`),
                        INDEX `idx_fecha_hora` (`FechaHora`),
                        INDEX `idx_alimento_id` (`AlimentoId`),
                        CONSTRAINT `fk_registro_alimento` 
                            FOREIGN KEY (`AlimentoId`) 
                            REFERENCES `Alimentos` (`Id`) 
                            ON DELETE CASCADE 
                            ON UPDATE CASCADE
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

                await connection.ExecuteNonQueryAsync(createRegistroComidasTable);
                _logger.LogInformation("Tabla 'RegistroComidas' verificada/creada correctamente");
            }

            _logger.LogInformation("Inicialización de la base de datos completada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar la base de datos");
            throw;
        }
    }
}

/// <summary>
/// Extensiones para MySqlConnection para ejecutar comandos SQL
/// </summary>
internal static class MySqlConnectionExtensions
{
    public static async Task<int> ExecuteNonQueryAsync(this MySqlConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return await command.ExecuteNonQueryAsync();
    }
}

