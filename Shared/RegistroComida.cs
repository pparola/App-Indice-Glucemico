namespace App_Indice_Glucemico.Shared;

/// <summary>
/// Representa una comida específica que el usuario ha consumido
/// </summary>
public class RegistroComida
{
    /// <summary>
    /// Identificador único del registro (Primary Key)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identificador del alimento consumido (Foreign Key a Alimento)
    /// </summary>
    public int AlimentoId { get; set; }

    /// <summary>
    /// Fecha y hora en que se consumió el alimento
    /// </summary>
    public DateTime FechaHora { get; set; }

    /// <summary>
    /// Cantidad consumida en gramos
    /// </summary>
    public decimal GramosConsumidos { get; set; }

    /// <summary>
    /// Tipo de comida (Desayuno, Almuerzo, Cena, Snack)
    /// </summary>
    public TipoComida TipoComida { get; set; }

    /// <summary>
    /// Carga glucémica calculada (opcional - se puede almacenar o calcular al vuelo)
    /// </summary>
    public decimal? CargaGlucemicaCalculada { get; set; }

    /// <summary>
    /// Navegación al alimento relacionado
    /// </summary>
    public virtual Alimento? Alimento { get; set; }
}

