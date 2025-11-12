using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using App_Indice_Glucemico.Server.Controllers;
using App_Indice_Glucemico.Shared;

namespace App_Indice_Glucemico.Tests.Controllers;

/// <summary>
/// Tests unitarios para AlimentosController
/// </summary>
public class AlimentosControllerTests
{
    private readonly Mock<IAlimentoRepository> _mockRepository;
    private readonly Mock<ILogger<AlimentosController>> _mockLogger;
    private readonly AlimentosController _controller;

    public AlimentosControllerTests()
    {
        _mockRepository = new Mock<IAlimentoRepository>();
        _mockLogger = new Mock<ILogger<AlimentosController>>();
        _controller = new AlimentosController(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithListOfAlimentos()
    {
        // Arrange
        var alimentos = new List<Alimento>
        {
            new Alimento { Id = 1, Nombre = "Manzana", IndiceGlucemico = 38, CarbsPor100g = 14.5m },
            new Alimento { Id = 2, Nombre = "Banana", IndiceGlucemico = 51, CarbsPor100g = 23.0m }
        };

        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(alimentos);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<Alimento>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
    }

    [Fact]
    public async Task GetAll_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAll();

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult_WithAlimento()
    {
        // Arrange
        var alimento = new Alimento 
        { 
            Id = 1, 
            Nombre = "Manzana", 
            IndiceGlucemico = 38, 
            CarbsPor100g = 14.5m 
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(alimento);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<Alimento>(okResult.Value);
        Assert.Equal(1, returnValue.Id);
        Assert.Equal("Manzana", returnValue.Nombre);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenAlimentoDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Alimento?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Contains("999", notFoundResult.Value?.ToString() ?? "");
    }

    [Fact]
    public async Task SearchByName_ReturnsOkResult_WithMatchingAlimentos()
    {
        // Arrange
        var alimentos = new List<Alimento>
        {
            new Alimento { Id = 1, Nombre = "Manzana Roja", IndiceGlucemico = 38, CarbsPor100g = 14.5m },
            new Alimento { Id = 2, Nombre = "Manzana Verde", IndiceGlucemico = 39, CarbsPor100g = 13.0m }
        };

        _mockRepository.Setup(r => r.SearchByNameAsync("Manzana")).ReturnsAsync(alimentos);

        // Act
        var result = await _controller.SearchByName("Manzana");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<Alimento>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
    }

    [Fact]
    public async Task SearchByName_ReturnsBadRequest_WhenNombreIsEmpty()
    {
        // Act
        var result = await _controller.SearchByName("");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedResult_WithAlimento()
    {
        // Arrange
        var nuevoAlimento = new Alimento
        {
            Nombre = "Nuevo Alimento",
            IndiceGlucemico = 50,
            CarbsPor100g = 20.0m,
            FuenteDatos = "TEST"
        };

        var alimentoCreado = new Alimento
        {
            Id = 1,
            Nombre = "Nuevo Alimento",
            IndiceGlucemico = 50,
            CarbsPor100g = 20.0m,
            FuenteDatos = "TEST"
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Alimento>())).ReturnsAsync(alimentoCreado);

        // Act
        var result = await _controller.Create(nuevoAlimento);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnValue = Assert.IsType<Alimento>(createdResult.Value);
        Assert.Equal(1, returnValue.Id);
        Assert.Equal("Nuevo Alimento", returnValue.Nombre);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenAlimentoIsNull()
    {
        // Act
        var result = await _controller.Create(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenNombreIsEmpty()
    {
        // Arrange
        var alimento = new Alimento { Nombre = "", IndiceGlucemico = 50, CarbsPor100g = 20.0m };

        // Act
        var result = await _controller.Create(alimento);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenIndiceGlucemicoIsNegative()
    {
        // Arrange
        var alimento = new Alimento { Nombre = "Test", IndiceGlucemico = -1, CarbsPor100g = 20.0m };

        // Act
        var result = await _controller.Create(alimento);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenCarbsPor100gIsNegative()
    {
        // Arrange
        var alimento = new Alimento { Nombre = "Test", IndiceGlucemico = 50, CarbsPor100g = -1m };

        // Act
        var result = await _controller.Create(alimento);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenUpdateIsSuccessful()
    {
        // Arrange
        var alimento = new Alimento
        {
            Id = 1,
            Nombre = "Alimento Actualizado",
            IndiceGlucemico = 50,
            CarbsPor100g = 20.0m
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(alimento);
        _mockRepository.Setup(r => r.UpdateAsync(alimento)).ReturnsAsync(true);

        // Act
        var result = await _controller.Update(1, alimento);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenIdMismatch()
    {
        // Arrange
        var alimento = new Alimento { Id = 2, Nombre = "Test", IndiceGlucemico = 50, CarbsPor100g = 20.0m };

        // Act
        var result = await _controller.Update(1, alimento);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenAlimentoDoesNotExist()
    {
        // Arrange
        var alimento = new Alimento { Id = 999, Nombre = "Test", IndiceGlucemico = 50, CarbsPor100g = 20.0m };
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Alimento?)null);

        // Act
        var result = await _controller.Update(999, alimento);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleteIsSuccessful()
    {
        // Arrange
        var alimento = new Alimento { Id = 1, Nombre = "Test", IndiceGlucemico = 50, CarbsPor100g = 20.0m };
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(alimento);
        _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenAlimentoDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Alimento?)null);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
    }
}

