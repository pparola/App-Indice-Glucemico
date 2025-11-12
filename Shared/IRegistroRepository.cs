namespace App_Indice_Glucemico.Shared;

/// <summary>
/// Interfaz para el repositorio de Registros de Comida
/// </summary>
public interface IRegistroRepository
{
    /// <summary>
    /// Obtiene un registro por su ID
    /// </summary>
    Task<RegistroComida?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene todos los registros en un rango de fechas
    /// </summary>
    Task<IEnumerable<RegistroComida>> GetByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin);

    /// <summary>
    /// Obtiene todos los registros de un día específico
    /// </summary>
    Task<IEnumerable<RegistroComida>> GetByDateAsync(DateTime fecha);

    /// <summary>
    /// Obtiene todos los registros del día actual
    /// </summary>
    Task<IEnumerable<RegistroComida>> GetTodayAsync();

    /// <summary>
    /// Agrega un nuevo registro de comida
    /// </summary>
    Task<RegistroComida> AddAsync(RegistroComida registro);

    /// <summary>
    /// Elimina un registro por su ID
    /// </summary>
    Task<bool> DeleteAsync(int id);
}

