using Dapper;
using MySqlConnector;
using App_Indice_Glucemico.Shared;

namespace App_Indice_Glucemico.Server.Repositories;

/// <summary>
/// Implementación del repositorio de Registros de Comida usando Dapper
/// </summary>
public class RegistroRepository : IRegistroRepository
{
    private readonly string _connectionString;

    public RegistroRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("app_indice_glucemico") 
            ?? throw new InvalidOperationException("Connection string 'app_indice_glucemico' not found.");
    }

    /// <summary>
    /// Obtiene un registro por su ID
    /// </summary>
    public async Task<RegistroComida?> GetByIdAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        // Obtener el registro - usar un tipo anónimo para mapear correctamente el enum
        const string sqlRegistro = @"
            SELECT Id, UsuarioId, AlimentoId, FechaHora, GramosConsumidos, TipoComida, CargaGlucemicaCalculada
            FROM RegistroComidas
            WHERE Id = @Id";
        
        var registroData = await connection.QueryFirstOrDefaultAsync<dynamic>(sqlRegistro, new { Id = id });
        
        if (registroData == null)
            return null;
        
        // Mapear manualmente para asegurar que el enum se convierta correctamente
        var registro = new RegistroComida
        {
            Id = registroData.Id,
            UsuarioId = registroData.UsuarioId,
            AlimentoId = registroData.AlimentoId,
            FechaHora = registroData.FechaHora,
            GramosConsumidos = registroData.GramosConsumidos,
            TipoComida = (TipoComida)(int)registroData.TipoComida,
            CargaGlucemicaCalculada = registroData.CargaGlucemicaCalculada
        };
        
        // Obtener el usuario relacionado
        const string sqlUsuario = @"
            SELECT Id, Nombre, Email, Password, FechaCreacion, Activo
            FROM Usuarios
            WHERE Id = @UsuarioId";
        
        registro.Usuario = await connection.QueryFirstOrDefaultAsync<Usuario>(sqlUsuario, new { UsuarioId = registro.UsuarioId });
        
        // Obtener el alimento relacionado
        const string sqlAlimento = @"
            SELECT Id, Nombre, IndiceGlucemico, CarbsPor100g, FuenteDatos
            FROM Alimentos
            WHERE Id = @AlimentoId";
        
        registro.Alimento = await connection.QueryFirstOrDefaultAsync<Alimento>(sqlAlimento, new { AlimentoId = registro.AlimentoId });
        
        return registro;
    }

    /// <summary>
    /// Obtiene todos los registros en un rango de fechas
    /// </summary>
    public async Task<IEnumerable<RegistroComida>> GetByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        // Obtener los registros
        const string sqlRegistros = @"
            SELECT Id, UsuarioId, AlimentoId, FechaHora, GramosConsumidos, TipoComida, CargaGlucemicaCalculada
            FROM RegistroComidas
            WHERE DATE(FechaHora) BETWEEN @FechaInicio AND @FechaFin
            ORDER BY FechaHora DESC";
        
        var registrosData = (await connection.QueryAsync<dynamic>(
            sqlRegistros, 
            new { FechaInicio = fechaInicio.Date, FechaFin = fechaFin.Date })).ToList();
        
        if (!registrosData.Any())
            return new List<RegistroComida>();
        
        // Mapear los registros manualmente para asegurar que el enum se convierta correctamente
        var registros = registrosData.Select(r => new RegistroComida
        {
            Id = r.Id,
            UsuarioId = r.UsuarioId,
            AlimentoId = r.AlimentoId,
            FechaHora = r.FechaHora,
            GramosConsumidos = r.GramosConsumidos,
            TipoComida = (TipoComida)(int)r.TipoComida,
            CargaGlucemicaCalculada = r.CargaGlucemicaCalculada
        }).ToList();
        
        // Obtener los alimentos relacionados
        var alimentoIds = registros.Select(r => r.AlimentoId).Distinct().ToList();
        const string sqlAlimentos = @"
            SELECT Id, Nombre, IndiceGlucemico, CarbsPor100g, FuenteDatos
            FROM Alimentos
            WHERE Id IN @AlimentoIds";
        
        var alimentos = (await connection.QueryAsync<Alimento>(
            sqlAlimentos, 
            new { AlimentoIds = alimentoIds })).ToDictionary(a => a.Id);
        
        // Asignar los alimentos a los registros
        foreach (var registro in registros)
        {
            if (alimentos.TryGetValue(registro.AlimentoId, out var alimento))
            {
                registro.Alimento = alimento;
            }
        }
        
        return registros;
    }

    /// <summary>
    /// Obtiene todos los registros en un rango de fechas para un usuario específico
    /// </summary>
    public async Task<IEnumerable<RegistroComida>> GetByDateRangeAsync(int usuarioId, DateTime fechaInicio, DateTime fechaFin)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        // Obtener los registros
        const string sqlRegistros = @"
            SELECT Id, UsuarioId, AlimentoId, FechaHora, GramosConsumidos, TipoComida, CargaGlucemicaCalculada
            FROM RegistroComidas
            WHERE UsuarioId = @UsuarioId AND DATE(FechaHora) BETWEEN @FechaInicio AND @FechaFin
            ORDER BY FechaHora DESC";
        
        var registrosData = (await connection.QueryAsync<dynamic>(
            sqlRegistros, 
            new { UsuarioId = usuarioId, FechaInicio = fechaInicio.Date, FechaFin = fechaFin.Date })).ToList();
        
        if (!registrosData.Any())
            return new List<RegistroComida>();
        
        // Mapear los registros manualmente para asegurar que el enum se convierta correctamente
        var registros = registrosData.Select(r => new RegistroComida
        {
            Id = r.Id,
            UsuarioId = r.UsuarioId,
            AlimentoId = r.AlimentoId,
            FechaHora = r.FechaHora,
            GramosConsumidos = r.GramosConsumidos,
            TipoComida = (TipoComida)(int)r.TipoComida,
            CargaGlucemicaCalculada = r.CargaGlucemicaCalculada
        }).ToList();
        
        // Obtener los alimentos relacionados
        var alimentoIds = registros.Select(r => r.AlimentoId).Distinct().ToList();
        const string sqlAlimentos = @"
            SELECT Id, Nombre, IndiceGlucemico, CarbsPor100g, FuenteDatos
            FROM Alimentos
            WHERE Id IN @AlimentoIds";
        
        var alimentos = (await connection.QueryAsync<Alimento>(
            sqlAlimentos, 
            new { AlimentoIds = alimentoIds })).ToDictionary(a => a.Id);
        
        // Asignar los alimentos a los registros
        foreach (var registro in registros)
        {
            if (alimentos.TryGetValue(registro.AlimentoId, out var alimento))
            {
                registro.Alimento = alimento;
            }
        }
        
        return registros;
    }

    /// <summary>
    /// Obtiene todos los registros de un día específico
    /// </summary>
    public async Task<IEnumerable<RegistroComida>> GetByDateAsync(DateTime fecha)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        // Obtener los registros
        const string sqlRegistros = @"
            SELECT Id, UsuarioId, AlimentoId, FechaHora, GramosConsumidos, TipoComida, CargaGlucemicaCalculada
            FROM RegistroComidas
            WHERE DATE(FechaHora) = @Fecha
            ORDER BY FechaHora DESC";
        
        var registrosData = (await connection.QueryAsync<dynamic>(
            sqlRegistros, 
            new { Fecha = fecha.Date })).ToList();
        
        if (!registrosData.Any())
            return new List<RegistroComida>();
        
        // Mapear los registros manualmente para asegurar que el enum se convierta correctamente
        var registros = registrosData.Select(r => new RegistroComida
        {
            Id = r.Id,
            UsuarioId = r.UsuarioId,
            AlimentoId = r.AlimentoId,
            FechaHora = r.FechaHora,
            GramosConsumidos = r.GramosConsumidos,
            TipoComida = (TipoComida)(int)r.TipoComida,
            CargaGlucemicaCalculada = r.CargaGlucemicaCalculada
        }).ToList();
        
        // Obtener los alimentos relacionados
        var alimentoIds = registros.Select(r => r.AlimentoId).Distinct().ToList();
        const string sqlAlimentos = @"
            SELECT Id, Nombre, IndiceGlucemico, CarbsPor100g, FuenteDatos
            FROM Alimentos
            WHERE Id IN @AlimentoIds";
        
        var alimentos = (await connection.QueryAsync<Alimento>(
            sqlAlimentos, 
            new { AlimentoIds = alimentoIds })).ToDictionary(a => a.Id);
        
        // Asignar los alimentos a los registros
        foreach (var registro in registros)
        {
            if (alimentos.TryGetValue(registro.AlimentoId, out var alimento))
            {
                registro.Alimento = alimento;
            }
        }
        
        return registros;
    }

    /// <summary>
    /// Obtiene todos los registros de un día específico para un usuario
    /// </summary>
    public async Task<IEnumerable<RegistroComida>> GetByDateAsync(int usuarioId, DateTime fecha)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        // Obtener los registros
        const string sqlRegistros = @"
            SELECT Id, UsuarioId, AlimentoId, FechaHora, GramosConsumidos, TipoComida, CargaGlucemicaCalculada
            FROM RegistroComidas
            WHERE UsuarioId = @UsuarioId AND DATE(FechaHora) = @Fecha
            ORDER BY FechaHora DESC";
        
        var registrosData = (await connection.QueryAsync<dynamic>(
            sqlRegistros, 
            new { UsuarioId = usuarioId, Fecha = fecha.Date })).ToList();
        
        if (!registrosData.Any())
            return new List<RegistroComida>();
        
        // Mapear los registros manualmente para asegurar que el enum se convierta correctamente
        var registros = registrosData.Select(r => new RegistroComida
        {
            Id = r.Id,
            UsuarioId = r.UsuarioId,
            AlimentoId = r.AlimentoId,
            FechaHora = r.FechaHora,
            GramosConsumidos = r.GramosConsumidos,
            TipoComida = (TipoComida)(int)r.TipoComida,
            CargaGlucemicaCalculada = r.CargaGlucemicaCalculada
        }).ToList();
        
        // Obtener los alimentos relacionados
        var alimentoIds = registros.Select(r => r.AlimentoId).Distinct().ToList();
        const string sqlAlimentos = @"
            SELECT Id, Nombre, IndiceGlucemico, CarbsPor100g, FuenteDatos
            FROM Alimentos
            WHERE Id IN @AlimentoIds";
        
        var alimentos = (await connection.QueryAsync<Alimento>(
            sqlAlimentos, 
            new { AlimentoIds = alimentoIds })).ToDictionary(a => a.Id);
        
        // Asignar los alimentos a los registros
        foreach (var registro in registros)
        {
            if (alimentos.TryGetValue(registro.AlimentoId, out var alimento))
            {
                registro.Alimento = alimento;
            }
        }
        
        return registros;
    }

    /// <summary>
    /// Obtiene todos los registros del día actual
    /// </summary>
    public async Task<IEnumerable<RegistroComida>> GetTodayAsync()
    {
        return await GetByDateAsync(DateTime.Now);
    }

    /// <summary>
    /// Obtiene todos los registros del día actual para un usuario específico
    /// </summary>
    public async Task<IEnumerable<RegistroComida>> GetTodayAsync(int usuarioId)
    {
        return await GetByDateAsync(usuarioId, DateTime.Now);
    }

    /// <summary>
    /// Agrega un nuevo registro de comida
    /// </summary>
    public async Task<RegistroComida> AddAsync(RegistroComida registro)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        const string sql = @"
            INSERT INTO RegistroComidas (UsuarioId, AlimentoId, FechaHora, GramosConsumidos, TipoComida, CargaGlucemicaCalculada) 
            VALUES (@UsuarioId, @AlimentoId, @FechaHora, @GramosConsumidos, @TipoComida, @CargaGlucemicaCalculada);
            SELECT LAST_INSERT_ID();";

        var id = await connection.ExecuteScalarAsync<long>(sql, new
        {
            registro.UsuarioId,
            registro.AlimentoId,
            registro.FechaHora,
            registro.GramosConsumidos,
            TipoComida = (int)registro.TipoComida,
            registro.CargaGlucemicaCalculada
        });

        registro.Id = (int)id;
        return registro;
    }

    /// <summary>
    /// Elimina un registro por su ID
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        const string sql = "DELETE FROM RegistroComidas WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }
}

