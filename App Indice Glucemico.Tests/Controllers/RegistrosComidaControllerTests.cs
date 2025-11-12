using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using App_Indice_Glucemico.Server.Controllers;
using App_Indice_Glucemico.Shared;

namespace App_Indice_Glucemico.Tests.Controllers;

/// <summary>
/// Tests unitarios para RegistrosComidaController
/// </summary>
public class RegistrosComidaControllerTests
{
    private readonly Mock<IRegistroRepository> _mockRegistroRepository;
    private readonly Mock<IAlimentoRepository> _mockAlimentoRepository;
    private readonly Mock<ILogger<RegistrosComidaController>> _mockLogger;
    private readonly RegistrosComidaController _controller;

    public RegistrosComidaControllerTests()
    {
        _mockRegistroRepository = new Mock<IRegistroRepository>();
        _mockAlimentoRepository = new Mock<IAlimentoRepository>();
        _mockLogger = new Mock<ILogger<RegistrosComidaController>>();
        _controller = new RegistrosComidaController(
            _mockRegistroRepository.Object,
            _mockAlimentoRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetToday_ReturnsOkResult_WithListOfRegistros()
    {
        // Arrange
        var alimento = new Alimento { Id = 1, Nombre = "Manzana", IndiceGlucemico = 38, CarbsPor100g = 14.5m };
        var registros = new List<RegistroComida>
        {
            new RegistroComida 
            { 
                Id = 1, 
                AlimentoId = 1, 
                FechaHora = DateTime.Today,
                GramosConsumidos = 150.0m,
                TipoComida = TipoComida.Desayuno,
                Alimento = alimento
            }
        };

        _mockRegistroRepository.Setup(r => r.GetTodayAsync()).ReturnsAsync(registros);

        // Act
        var result = await _controller.GetToday();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<RegistroComida>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult_WithRegistro()
    {
        // Arrange
        var alimento = new Alimento { Id = 1, Nombre = "Manzana", IndiceGlucemico = 38, CarbsPor100g = 14.5m };
        var registro = new RegistroComida
        {
            Id = 1,
            AlimentoId = 1,
            FechaHora = DateTime.Now,
            GramosConsumidos = 150.0m,
            TipoComida = TipoComida.Desayuno,
            Alimento = alimento
        };

        _mockRegistroRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(registro);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<RegistroComida>(okResult.Value);
        Assert.Equal(1, returnValue.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenRegistroDoesNotExist()
    {
        // Arrange
        _mockRegistroRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((RegistroComida?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByDate_ReturnsOkResult_WithRegistros()
    {
        // Arrange
        var fecha = DateTime.Today;
        var alimento = new Alimento { Id = 1, Nombre = "Manzana", IndiceGlucemico = 38, CarbsPor100g = 14.5m };
        var registros = new List<RegistroComida>
        {
            new RegistroComida 
            { 
                Id = 1, 
                AlimentoId = 1, 
                FechaHora = fecha,
                GramosConsumidos = 150.0m,
                TipoComida = TipoComida.Desayuno,
                Alimento = alimento
            }
        };

        _mockRegistroRepository.Setup(r => r.GetByDateAsync(fecha)).ReturnsAsync(registros);

        // Act
        var result = await _controller.GetByDate(fecha.ToString("yyyy-MM-dd"));

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<RegistroComida>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetByDate_ReturnsBadRequest_WhenDateFormatIsInvalid()
    {
        // Act
        var result = await _controller.GetByDate("invalid-date");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByDateRange_ReturnsOkResult_WithRegistros()
    {
        // Arrange
        var fechaInicio = DateTime.Today.AddDays(-5);
        var fechaFin = DateTime.Today;
        var alimento = new Alimento { Id = 1, Nombre = "Manzana", IndiceGlucemico = 38, CarbsPor100g = 14.5m };
        var registros = new List<RegistroComida>
        {
            new RegistroComida 
            { 
                Id = 1, 
                AlimentoId = 1, 
                FechaHora = fechaInicio.AddDays(2),
                GramosConsumidos = 150.0m,
                TipoComida = TipoComida.Almuerzo,
                Alimento = alimento
            }
        };

        _mockRegistroRepository.Setup(r => r.GetByDateRangeAsync(fechaInicio, fechaFin)).ReturnsAsync(registros);

        // Act
        var result = await _controller.GetByDateRange(
            fechaInicio.ToString("yyyy-MM-dd"),
            fechaFin.ToString("yyyy-MM-dd"));

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<RegistroComida>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetByDateRange_ReturnsBadRequest_WhenFechaInicioIsAfterFechaFin()
    {
        // Act
        var result = await _controller.GetByDateRange(
            DateTime.Today.ToString("yyyy-MM-dd"),
            DateTime.Today.AddDays(-5).ToString("yyyy-MM-dd"));

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedResult_WithRegistro()
    {
        // Arrange
        var alimento = new Alimento { Id = 1, Nombre = "Manzana", IndiceGlucemico = 38, CarbsPor100g = 14.5m };
        var nuevoRegistro = new RegistroComida
        {
            AlimentoId = 1,
            FechaHora = DateTime.Now,
            GramosConsumidos = 150.0m,
            TipoComida = TipoComida.Desayuno
        };

        var registroCreado = new RegistroComida
        {
            Id = 1,
            AlimentoId = 1,
            FechaHora = nuevoRegistro.FechaHora,
            GramosConsumidos = 150.0m,
            TipoComida = TipoComida.Desayuno,
            CargaGlucemicaCalculada = 8.265m
        };

        _mockAlimentoRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(alimento);
        _mockRegistroRepository.Setup(r => r.AddAsync(It.IsAny<RegistroComida>())).ReturnsAsync(registroCreado);

        // Act
        var result = await _controller.Create(nuevoRegistro);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnValue = Assert.IsType<RegistroComida>(createdResult.Value);
        Assert.Equal(1, returnValue.Id);
        Assert.NotNull(returnValue.CargaGlucemicaCalculada);
    }

    [Fact]
    public async Task Create_CalculatesCargaGlucemica_WhenNotProvided()
    {
        // Arrange
        var alimento = new Alimento { Id = 1, Nombre = "Manzana", IndiceGlucemico = 38, CarbsPor100g = 14.5m };
        var nuevoRegistro = new RegistroComida
        {
            AlimentoId = 1,
            FechaHora = DateTime.Now,
            GramosConsumidos = 150.0m,
            TipoComida = TipoComida.Desayuno,
            CargaGlucemicaCalculada = null
        };

        var registroCreado = new RegistroComida
        {
            Id = 1,
            AlimentoId = 1,
            FechaHora = nuevoRegistro.FechaHora,
            GramosConsumidos = 150.0m,
            TipoComida = TipoComida.Desayuno,
            CargaGlucemicaCalculada = 8.265m
        };

        _mockAlimentoRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(alimento);
        _mockRegistroRepository.Setup(r => r.AddAsync(It.Is<RegistroComida>(reg => 
            reg.CargaGlucemicaCalculada.HasValue && 
            reg.CargaGlucemicaCalculada.Value > 0))).ReturnsAsync(registroCreado);

        // Act
        var result = await _controller.Create(nuevoRegistro);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        _mockRegistroRepository.Verify(r => r.AddAsync(It.Is<RegistroComida>(reg => 
            reg.CargaGlucemicaCalculada.HasValue)), Times.Once);
    }

    [Fact]
    public async Task Create_SetsFechaHoraToNow_WhenNotProvided()
    {
        // Arrange
        var alimento = new Alimento { Id = 1, Nombre = "Manzana", IndiceGlucemico = 38, CarbsPor100g = 14.5m };
        var nuevoRegistro = new RegistroComida
        {
            AlimentoId = 1,
            FechaHora = default,
            GramosConsumidos = 150.0m,
            TipoComida = TipoComida.Desayuno
        };

        var registroCreado = new RegistroComida
        {
            Id = 1,
            AlimentoId = 1,
            FechaHora = DateTime.Now,
            GramosConsumidos = 150.0m,
            TipoComida = TipoComida.Desayuno
        };

        _mockAlimentoRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(alimento);
        _mockRegistroRepository.Setup(r => r.AddAsync(It.IsAny<RegistroComida>())).ReturnsAsync(registroCreado);

        // Act
        var result = await _controller.Create(nuevoRegistro);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        _mockRegistroRepository.Verify(r => r.AddAsync(It.Is<RegistroComida>(reg => 
            reg.FechaHora != default)), Times.Once);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenRegistroIsNull()
    {
        // Act
        var result = await _controller.Create(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenAlimentoDoesNotExist()
    {
        // Arrange
        var nuevoRegistro = new RegistroComida
        {
            AlimentoId = 999,
            FechaHora = DateTime.Now,
            GramosConsumidos = 150.0m,
            TipoComida = TipoComida.Desayuno
        };

        _mockAlimentoRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Alimento?)null);

        // Act
        var result = await _controller.Create(nuevoRegistro);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenGramosConsumidosIsZeroOrNegative()
    {
        // Arrange
        var alimento = new Alimento { Id = 1, Nombre = "Manzana", IndiceGlucemico = 38, CarbsPor100g = 14.5m };
        var nuevoRegistro = new RegistroComida
        {
            AlimentoId = 1,
            FechaHora = DateTime.Now,
            GramosConsumidos = 0m,
            TipoComida = TipoComida.Desayuno
        };

        _mockAlimentoRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(alimento);

        // Act
        var result = await _controller.Create(nuevoRegistro);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleteIsSuccessful()
    {
        // Arrange
        var alimento = new Alimento { Id = 1, Nombre = "Manzana", IndiceGlucemico = 38, CarbsPor100g = 14.5m };
        var registro = new RegistroComida
        {
            Id = 1,
            AlimentoId = 1,
            FechaHora = DateTime.Now,
            GramosConsumidos = 150.0m,
            TipoComida = TipoComida.Desayuno,
            Alimento = alimento
        };

        _mockRegistroRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(registro);
        _mockRegistroRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenRegistroDoesNotExist()
    {
        // Arrange
        _mockRegistroRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((RegistroComida?)null);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
    }
}

