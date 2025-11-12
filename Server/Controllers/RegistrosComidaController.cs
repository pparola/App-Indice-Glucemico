using Microsoft.AspNetCore.Mvc;
using App_Indice_Glucemico.Shared;

namespace App_Indice_Glucemico.Server.Controllers;

/// <summary>
/// Controller para gestionar registros de comida
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RegistrosComidaController : ControllerBase
{
    private readonly IRegistroRepository _registroRepository;
    private readonly IAlimentoRepository _alimentoRepository;
    private readonly ILogger<RegistrosComidaController> _logger;

    public RegistrosComidaController(
        IRegistroRepository registroRepository,
        IAlimentoRepository alimentoRepository,
        ILogger<RegistrosComidaController> logger)
    {
        _registroRepository = registroRepository;
        _alimentoRepository = alimentoRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los registros del día actual
    /// </summary>
    /// <returns>Lista de registros del día actual</returns>
    [HttpGet("hoy")]
    public async Task<ActionResult<IEnumerable<RegistroComida>>> GetToday()
    {
        try
        {
            var registros = await _registroRepository.GetTodayAsync();
            return Ok(registros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los registros de hoy");
            return StatusCode(500, "Error interno del servidor al obtener los registros de hoy");
        }
    }

    /// <summary>
    /// Obtiene un registro por su ID
    /// </summary>
    /// <param name="id">ID del registro</param>
    /// <returns>Registro encontrado</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<RegistroComida>> GetById(int id)
    {
        try
        {
            var registro = await _registroRepository.GetByIdAsync(id);
            
            if (registro == null)
            {
                return NotFound($"No se encontró el registro con ID {id}");
            }

            return Ok(registro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el registro con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor al obtener el registro");
        }
    }

    /// <summary>
    /// Obtiene todos los registros de una fecha específica
    /// </summary>
    /// <param name="fecha">Fecha en formato YYYY-MM-DD</param>
    /// <returns>Lista de registros de la fecha especificada</returns>
    [HttpGet("fecha/{fecha}")]
    public async Task<ActionResult<IEnumerable<RegistroComida>>> GetByDate(string fecha)
    {
        try
        {
            if (!DateTime.TryParse(fecha, out var fechaParseada))
            {
                return BadRequest("Formato de fecha inválido. Use el formato YYYY-MM-DD");
            }

            var registros = await _registroRepository.GetByDateAsync(fechaParseada);
            return Ok(registros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los registros de la fecha {Fecha}", fecha);
            return StatusCode(500, "Error interno del servidor al obtener los registros");
        }
    }

    /// <summary>
    /// Obtiene todos los registros en un rango de fechas
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio en formato YYYY-MM-DD</param>
    /// <param name="fechaFin">Fecha de fin en formato YYYY-MM-DD</param>
    /// <returns>Lista de registros en el rango de fechas</returns>
    [HttpGet("rango")]
    public async Task<ActionResult<IEnumerable<RegistroComida>>> GetByDateRange(
        [FromQuery] string fechaInicio,
        [FromQuery] string fechaFin)
    {
        try
        {
            if (!DateTime.TryParse(fechaInicio, out var fechaInicioParseada))
            {
                return BadRequest("Formato de fecha de inicio inválido. Use el formato YYYY-MM-DD");
            }

            if (!DateTime.TryParse(fechaFin, out var fechaFinParseada))
            {
                return BadRequest("Formato de fecha de fin inválido. Use el formato YYYY-MM-DD");
            }

            if (fechaInicioParseada > fechaFinParseada)
            {
                return BadRequest("La fecha de inicio debe ser anterior o igual a la fecha de fin");
            }

            var registros = await _registroRepository.GetByDateRangeAsync(fechaInicioParseada, fechaFinParseada);
            return Ok(registros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los registros en el rango {FechaInicio} - {FechaFin}", fechaInicio, fechaFin);
            return StatusCode(500, "Error interno del servidor al obtener los registros");
        }
    }

    /// <summary>
    /// Crea un nuevo registro de comida
    /// </summary>
    /// <param name="registro">Datos del registro a crear</param>
    /// <returns>Registro creado</returns>
    [HttpPost]
    public async Task<ActionResult<RegistroComida>> Create([FromBody] RegistroComida registro)
    {
        try
        {
            if (registro == null)
            {
                return BadRequest("El registro no puede ser nulo");
            }

            // Validar que el alimento existe
            var alimento = await _alimentoRepository.GetByIdAsync(registro.AlimentoId);
            if (alimento == null)
            {
                return BadRequest($"No se encontró el alimento con ID {registro.AlimentoId}");
            }

            if (registro.GramosConsumidos <= 0)
            {
                return BadRequest("Los gramos consumidos deben ser mayores a 0");
            }

            if (!Enum.IsDefined(typeof(TipoComida), registro.TipoComida))
            {
                return BadRequest("El tipo de comida no es válido");
            }

            // Si no se proporciona la fecha, usar la fecha y hora actual
            if (registro.FechaHora == default)
            {
                registro.FechaHora = DateTime.Now;
            }

            // Calcular la carga glucémica si no se proporciona
            if (!registro.CargaGlucemicaCalculada.HasValue)
            {
                registro.CargaGlucemicaCalculada = CalcularCargaGlucemica(alimento, registro.GramosConsumidos);
            }

            var registroCreado = await _registroRepository.AddAsync(registro);
            return CreatedAtAction(nameof(GetById), new { id = registroCreado.Id }, registroCreado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el registro");
            return StatusCode(500, "Error interno del servidor al crear el registro");
        }
    }

    /// <summary>
    /// Elimina un registro por su ID
    /// </summary>
    /// <param name="id">ID del registro a eliminar</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            // Verificar que el registro existe
            var registro = await _registroRepository.GetByIdAsync(id);
            if (registro == null)
            {
                return NotFound($"No se encontró el registro con ID {id}");
            }

            var resultado = await _registroRepository.DeleteAsync(id);
            
            if (!resultado)
            {
                return StatusCode(500, "Error al eliminar el registro");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el registro con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor al eliminar el registro");
        }
    }

    /// <summary>
    /// Calcula la carga glucémica según la fórmula: CG = (IG * GramosDeCarbsEnPorcion) / 100
    /// </summary>
    /// <param name="alimento">Alimento consumido</param>
    /// <param name="gramosConsumidos">Gramos consumidos</param>
    /// <returns>Carga glucémica calculada</returns>
    private decimal CalcularCargaGlucemica(Alimento alimento, decimal gramosConsumidos)
    {
        // Calcular gramos de carbohidratos en la porción
        var gramosDeCarbsEnPorcion = (alimento.CarbsPor100g / 100m) * gramosConsumidos;
        
        // Calcular carga glucémica
        var cargaGlucemica = (alimento.IndiceGlucemico * gramosDeCarbsEnPorcion) / 100m;
        
        return Math.Round(cargaGlucemica, 2);
    }
}

