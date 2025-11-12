using App_Indice_Glucemico.Server.Repositories;
using App_Indice_Glucemico.Shared;
using App_Indice_Glucemico.Tests.Helpers;
using Microsoft.Extensions.Configuration;

namespace App_Indice_Glucemico.Tests.Repositories;

/// <summary>
/// Tests de integración para AlimentoRepository
/// Estos tests se ejecutan contra la base de datos real
/// </summary>
[Collection("Database")]
public class AlimentoRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly IConfiguration _configuration;
    private readonly AlimentoRepository _repository;

    public AlimentoRepositoryTests(DatabaseFixture fixture)
    {
        _configuration = fixture.Configuration;
        _repository = new AlimentoRepository(_configuration);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsAlimento()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var alimentoTest = await DatabaseHelper.CreateTestAlimentoAsync(
            nombre: "Manzana Roja",
            indiceGlucemico: 38,
            carbsPor100g: 14.5m,
            fuenteDatos: "USDA");

        // Act
        var resultado = await _repository.GetByIdAsync(alimentoTest.Id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(alimentoTest.Id, resultado.Id);
        Assert.Equal("Manzana Roja", resultado.Nombre);
        Assert.Equal(38, resultado.IndiceGlucemico);
        Assert.Equal(14.5m, resultado.CarbsPor100g);
        Assert.Equal("USDA", resultado.FuenteDatos);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var idInexistente = 99999;

        // Act
        var resultado = await _repository.GetByIdAsync(idInexistente);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllAlimentos()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var alimento1 = await DatabaseHelper.CreateTestAlimentoAsync("Manzana", 38, 14.5m);
        var alimento2 = await DatabaseHelper.CreateTestAlimentoAsync("Banana", 51, 23.0m);
        var alimento3 = await DatabaseHelper.CreateTestAlimentoAsync("Arroz Blanco", 73, 28.0m);

        // Act
        var resultados = await _repository.GetAllAsync();
        var listaResultados = resultados.ToList();

        // Assert
        Assert.NotNull(listaResultados);
        Assert.True(listaResultados.Count >= 3);
        Assert.Contains(listaResultados, a => a.Id == alimento1.Id);
        Assert.Contains(listaResultados, a => a.Id == alimento2.Id);
        Assert.Contains(listaResultados, a => a.Id == alimento3.Id);
        
        // Verificar que estén ordenados por nombre
        var nombres = listaResultados.Select(a => a.Nombre).ToList();
        var nombresOrdenados = nombres.OrderBy(n => n).ToList();
        Assert.Equal(nombresOrdenados, nombres);
    }

    [Fact]
    public async Task SearchByNameAsync_WithPartialMatch_ReturnsMatchingAlimentos()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var alimento1 = await DatabaseHelper.CreateTestAlimentoAsync("Manzana Roja", 38, 14.5m);
        var alimento2 = await DatabaseHelper.CreateTestAlimentoAsync("Manzana Verde", 39, 13.0m);
        var alimento3 = await DatabaseHelper.CreateTestAlimentoAsync("Banana", 51, 23.0m);

        // Act
        var resultados = await _repository.SearchByNameAsync("Manzana");
        var listaResultados = resultados.ToList();

        // Assert
        Assert.NotNull(listaResultados);
        Assert.True(listaResultados.Count >= 2);
        Assert.Contains(listaResultados, a => a.Id == alimento1.Id);
        Assert.Contains(listaResultados, a => a.Id == alimento2.Id);
        Assert.DoesNotContain(listaResultados, a => a.Id == alimento3.Id);
    }

    [Fact]
    public async Task SearchByNameAsync_WithNoMatch_ReturnsEmptyList()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        await DatabaseHelper.CreateTestAlimentoAsync("Manzana", 38, 14.5m);

        // Act
        var resultados = await _repository.SearchByNameAsync("XYZ123NoExiste");
        var listaResultados = resultados.ToList();

        // Assert
        Assert.NotNull(listaResultados);
        Assert.Empty(listaResultados);
    }

    [Fact]
    public async Task AddAsync_CreatesNewAlimento()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var nuevoAlimento = new Alimento
        {
            Nombre = "Nuevo Alimento Test",
            IndiceGlucemico = 45,
            CarbsPor100g = 18.5m,
            FuenteDatos = "TEST"
        };

        // Act
        var resultado = await _repository.AddAsync(nuevoAlimento);

        // Assert
        Assert.NotNull(resultado);
        Assert.True(resultado.Id > 0);
        Assert.Equal("Nuevo Alimento Test", resultado.Nombre);
        Assert.Equal(45, resultado.IndiceGlucemico);
        Assert.Equal(18.5m, resultado.CarbsPor100g);
        Assert.Equal("TEST", resultado.FuenteDatos);
        
        // Verificar que existe en la base de datos
        var existe = await DatabaseHelper.AlimentoExistsAsync(resultado.Id);
        Assert.True(existe);
    }

    [Fact]
    public async Task UpdateAsync_WithValidAlimento_UpdatesSuccessfully()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var alimentoTest = await DatabaseHelper.CreateTestAlimentoAsync("Alimento Original", 40, 15.0m);
        
        var alimentoActualizado = new Alimento
        {
            Id = alimentoTest.Id,
            Nombre = "Alimento Actualizado",
            IndiceGlucemico = 50,
            CarbsPor100g = 20.0m,
            FuenteDatos = "TEST_UPDATED"
        };

        // Act
        var resultado = await _repository.UpdateAsync(alimentoActualizado);

        // Assert
        Assert.True(resultado);
        
        // Verificar los cambios
        var alimentoVerificado = await _repository.GetByIdAsync(alimentoTest.Id);
        Assert.NotNull(alimentoVerificado);
        Assert.Equal("Alimento Actualizado", alimentoVerificado.Nombre);
        Assert.Equal(50, alimentoVerificado.IndiceGlucemico);
        Assert.Equal(20.0m, alimentoVerificado.CarbsPor100g);
        Assert.Equal("TEST_UPDATED", alimentoVerificado.FuenteDatos);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var alimentoInexistente = new Alimento
        {
            Id = 99999,
            Nombre = "No Existe",
            IndiceGlucemico = 40,
            CarbsPor100g = 15.0m
        };

        // Act
        var resultado = await _repository.UpdateAsync(alimentoInexistente);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var alimentoTest = await DatabaseHelper.CreateTestAlimentoAsync("Alimento a Eliminar", 40, 15.0m);

        // Act
        var resultado = await _repository.DeleteAsync(alimentoTest.Id);

        // Assert
        Assert.True(resultado);
        
        // Verificar que ya no existe
        var existe = await DatabaseHelper.AlimentoExistsAsync(alimentoTest.Id);
        Assert.False(existe);
        
        // Verificar que GetByIdAsync retorna null
        var alimentoEliminado = await _repository.GetByIdAsync(alimentoTest.Id);
        Assert.Null(alimentoEliminado);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var idInexistente = 99999;

        // Act
        var resultado = await _repository.DeleteAsync(idInexistente);

        // Assert
        Assert.False(resultado);
    }
}

