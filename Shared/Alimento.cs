namespace App_Indice_Glucemico.Shared;

/// <summary>
/// Representa un ingrediente o alimento base en el catálogo
/// </summary>
public class Alimento
{
    /// <summary>
    /// Identificador único del alimento (Primary Key)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre del alimento (ej: "Manzana (Roja)")
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Índice glucémico del alimento (valor de 0 a 100+)
    /// </summary>
    public int IndiceGlucemico { get; set; }

    /// <summary>
    /// Gramos de carbohidratos por 100g del alimento
    /// </summary>
    public decimal CarbsPor100g { get; set; }

    /// <summary>
    /// Fuente de los datos (opcional, ej: "USDA")
    /// </summary>
    public string? FuenteDatos { get; set; }
}

