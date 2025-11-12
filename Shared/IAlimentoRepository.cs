namespace App_Indice_Glucemico.Shared;

/// <summary>
/// Interfaz para el repositorio de Alimentos
/// </summary>
public interface IAlimentoRepository
{
    /// <summary>
    /// Obtiene un alimento por su ID
    /// </summary>
    Task<Alimento?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene todos los alimentos
    /// </summary>
    Task<IEnumerable<Alimento>> GetAllAsync();

    /// <summary>
    /// Busca alimentos por nombre (búsqueda parcial)
    /// </summary>
    Task<IEnumerable<Alimento>> SearchByNameAsync(string nombre);

    /// <summary>
    /// Agrega un nuevo alimento
    /// </summary>
    Task<Alimento> AddAsync(Alimento alimento);

    /// <summary>
    /// Actualiza un alimento existente
    /// </summary>
    Task<bool> UpdateAsync(Alimento alimento);

    /// <summary>
    /// Elimina un alimento por su ID
    /// </summary>
    Task<bool> DeleteAsync(int id);
}

