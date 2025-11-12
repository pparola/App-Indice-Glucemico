using Microsoft.AspNetCore.Mvc;
using App_Indice_Glucemico.Shared;

namespace App_Indice_Glucemico.Server.Controllers;

/// <summary>
/// Controller para gestionar alimentos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AlimentosController : ControllerBase
{
    private readonly IAlimentoRepository _alimentoRepository;
    private readonly ILogger<AlimentosController> _logger;

    public AlimentosController(IAlimentoRepository alimentoRepository, ILogger<AlimentosController> logger)
    {
        _alimentoRepository = alimentoRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los alimentos
    /// </summary>
    /// <returns>Lista de alimentos</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Alimento>>> GetAll()
    {
        try
        {
            var alimentos = await _alimentoRepository.GetAllAsync();
            return Ok(alimentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los alimentos");
            return StatusCode(500, "Error interno del servidor al obtener los alimentos");
        }
    }

    /// <summary>
    /// Obtiene un alimento por su ID
    /// </summary>
    /// <param name="id">ID del alimento</param>
    /// <returns>Alimento encontrado</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Alimento>> GetById(int id)
    {
        try
        {
            var alimento = await _alimentoRepository.GetByIdAsync(id);
            
            if (alimento == null)
            {
                return NotFound($"No se encontró el alimento con ID {id}");
            }

            return Ok(alimento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el alimento con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor al obtener el alimento");
        }
    }

    /// <summary>
    /// Busca alimentos por nombre (búsqueda parcial)
    /// </summary>
    /// <param name="nombre">Nombre o parte del nombre a buscar</param>
    /// <returns>Lista de alimentos que coinciden con la búsqueda</returns>
    [HttpGet("buscar")]
    public async Task<ActionResult<IEnumerable<Alimento>>> SearchByName([FromQuery] string nombre)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest("El parámetro 'nombre' es requerido");
            }

            var alimentos = await _alimentoRepository.SearchByNameAsync(nombre);
            return Ok(alimentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar alimentos por nombre: {Nombre}", nombre);
            return StatusCode(500, "Error interno del servidor al buscar alimentos");
        }
    }

    /// <summary>
    /// Crea un nuevo alimento
    /// </summary>
    /// <param name="alimento">Datos del alimento a crear</param>
    /// <returns>Alimento creado</returns>
    [HttpPost]
    public async Task<ActionResult<Alimento>> Create([FromBody] Alimento alimento)
    {
        try
        {
            if (alimento == null)
            {
                return BadRequest("El alimento no puede ser nulo");
            }

            if (string.IsNullOrWhiteSpace(alimento.Nombre))
            {
                return BadRequest("El nombre del alimento es requerido");
            }

            if (alimento.IndiceGlucemico < 0)
            {
                return BadRequest("El índice glucémico debe ser mayor o igual a 0");
            }

            if (alimento.CarbsPor100g < 0)
            {
                return BadRequest("Los carbohidratos por 100g deben ser mayores o iguales a 0");
            }

            var alimentoCreado = await _alimentoRepository.AddAsync(alimento);
            return CreatedAtAction(nameof(GetById), new { id = alimentoCreado.Id }, alimentoCreado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el alimento");
            return StatusCode(500, "Error interno del servidor al crear el alimento");
        }
    }

    /// <summary>
    /// Actualiza un alimento existente
    /// </summary>
    /// <param name="id">ID del alimento a actualizar</param>
    /// <param name="alimento">Datos actualizados del alimento</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Alimento alimento)
    {
        try
        {
            if (alimento == null)
            {
                return BadRequest("El alimento no puede ser nulo");
            }

            if (id != alimento.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del alimento");
            }

            if (string.IsNullOrWhiteSpace(alimento.Nombre))
            {
                return BadRequest("El nombre del alimento es requerido");
            }

            if (alimento.IndiceGlucemico < 0)
            {
                return BadRequest("El índice glucémico debe ser mayor o igual a 0");
            }

            if (alimento.CarbsPor100g < 0)
            {
                return BadRequest("Los carbohidratos por 100g deben ser mayores o iguales a 0");
            }

            // Verificar que el alimento existe
            var alimentoExistente = await _alimentoRepository.GetByIdAsync(id);
            if (alimentoExistente == null)
            {
                return NotFound($"No se encontró el alimento con ID {id}");
            }

            var resultado = await _alimentoRepository.UpdateAsync(alimento);
            
            if (!resultado)
            {
                return StatusCode(500, "Error al actualizar el alimento");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el alimento con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor al actualizar el alimento");
        }
    }

    /// <summary>
    /// Elimina un alimento por su ID
    /// </summary>
    /// <param name="id">ID del alimento a eliminar</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            // Verificar que el alimento existe
            var alimento = await _alimentoRepository.GetByIdAsync(id);
            if (alimento == null)
            {
                return NotFound($"No se encontró el alimento con ID {id}");
            }

            var resultado = await _alimentoRepository.DeleteAsync(id);
            
            if (!resultado)
            {
                return StatusCode(500, "Error al eliminar el alimento");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el alimento con ID {Id}", id);
            return StatusCode(500, "Error interno del servidor al eliminar el alimento");
        }
    }
}

