using App_Indice_Glucemico.Server.Repositories;
using App_Indice_Glucemico.Shared;
using App_Indice_Glucemico.Tests.Helpers;
using Microsoft.Extensions.Configuration;

namespace App_Indice_Glucemico.Tests.Repositories;

/// <summary>
/// Tests de integración para RegistroRepository
/// Estos tests se ejecutan contra la base de datos real
/// </summary>
[Collection("Database")]
public class RegistroRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly IConfiguration _configuration;
    private readonly RegistroRepository _registroRepository;
    private readonly AlimentoRepository _alimentoRepository;

    public RegistroRepositoryTests(DatabaseFixture fixture)
    {
        _configuration = fixture.Configuration;
        _registroRepository = new RegistroRepository(_configuration);
        _alimentoRepository = new AlimentoRepository(_configuration);
    }

    [Fact]
    public async Task AddAsync_CreatesNewRegistro()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var alimento = await DatabaseHelper.CreateTestAlimentoAsync("Manzana", 38, 14.5m);
        
        var nuevoRegistro = new RegistroComida
        {
            AlimentoId = alimento.Id,
            FechaHora = DateTime.Now,
            GramosConsumidos = 150.0m,
            TipoComida = TipoComida.Desayuno,
            CargaGlucemicaCalculada = 8.265m
        };

        // Act
        var resultado = await _registroRepository.AddAsync(nuevoRegistro);

        // Assert
        Assert.NotNull(resultado);
        Assert.True(resultado.Id > 0);
        Assert.Equal(alimento.Id, resultado.AlimentoId);
        Assert.Equal(150.0m, resultado.GramosConsumidos);
        Assert.Equal(TipoComida.Desayuno, resultado.TipoComida);
        Assert.Equal(8.265m, resultado.CargaGlucemicaCalculada);
        
        // Verificar que existe en la base de datos
        var existe = await DatabaseHelper.RegistroExistsAsync(resultado.Id);
        Assert.True(existe);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsRegistroWithAlimento()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var alimento = await DatabaseHelper.CreateTestAlimentoAsync("Banana", 51, 23.0m);
        
        var registro = new RegistroComida
        {
            AlimentoId = alimento.Id,
            FechaHora = DateTime.Now,
            GramosConsumidos = 100.0m,
            TipoComida = TipoComida.Almuerzo,
            CargaGlucemicaCalculada = 11.73m
        };
        var registroCreado = await _registroRepository.AddAsync(registro);

        // Act
        var resultado = await _registroRepository.GetByIdAsync(registroCreado.Id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(registroCreado.Id, resultado.Id);
        Assert.Equal(alimento.Id, resultado.AlimentoId);
        Assert.Equal(100.0m, resultado.GramosConsumidos);
        Assert.Equal(TipoComida.Almuerzo, resultado.TipoComida);
        Assert.NotNull(resultado.Alimento);
        Assert.Equal("Banana", resultado.Alimento.Nombre);
        Assert.Equal(51, resultado.Alimento.IndiceGlucemico);
        Assert.Equal(23.0m, resultado.Alimento.CarbsPor100g);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var idInexistente = 99999;

        // Act
        var resultado = await _registroRepository.GetByIdAsync(idInexistente);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task GetByDateAsync_ReturnsRegistrosForSpecificDate()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var alimento = await DatabaseHelper.CreateTestAlimentoAsync("Manzana", 38, 14.5m);
        
        var fechaHoy = DateTime.Today;
        var fechaAyer = fechaHoy.AddDays(-1);
        
        var registroHoy1 = new RegistroComida
        {
            AlimentoId = alimento.Id,
            FechaHora = fechaHoy.AddHours(8), // 8 AM
            GramosConsumidos = 150.0m,
            TipoComida = TipoComida.Desayuno
        };
        await _registroRepository.AddAsync(registroHoy1);
        
        var registroHoy2 = new RegistroComida
        {
            AlimentoId = alimento.Id,
            FechaHora = fechaHoy.AddHours(13), // 1 PM
            GramosConsumidos = 200.0m,
            TipoComida = TipoComida.Almuerzo
        };
        await _registroRepository.AddAsync(registroHoy2);
        
        var registroAyer = new RegistroComida
        {
            AlimentoId = alimento.Id,
            FechaHora = fechaAyer.AddHours(12),
            GramosConsumidos = 100.0m,
            TipoComida = TipoComida.Cena
        };
        await _registroRepository.AddAsync(registroAyer);

        // Act
        var resultados = await _registroRepository.GetByDateAsync(fechaHoy);
        var listaResultados = resultados.ToList();

        // Assert
        Assert.NotNull(listaResultados);
        Assert.True(listaResultados.Count >= 2);
        Assert.All(listaResultados, r => Assert.Equal(fechaHoy.Date, r.FechaHora.Date));
        
        // Verificar que los registros tienen el alimento cargado
        Assert.All(listaResultados, r => Assert.NotNull(r.Alimento));
    }

    [Fact]
    public async Task GetTodayAsync_ReturnsRegistrosForToday()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var alimento = await DatabaseHelper.CreateTestAlimentoAsync("Banana", 51, 23.0m);
        
        var registro = new RegistroComida
        {
            AlimentoId = alimento.Id,
            FechaHora = DateTime.Now,
            GramosConsumidos = 120.0m,
            TipoComida = TipoComida.Snack
        };
        await _registroRepository.AddAsync(registro);

        // Act
        var resultados = await _registroRepository.GetTodayAsync();
        var listaResultados = resultados.ToList();

        // Assert
        Assert.NotNull(listaResultados);
        Assert.True(listaResultados.Count >= 1);
        Assert.All(listaResultados, r => Assert.Equal(DateTime.Today, r.FechaHora.Date));
        Assert.Contains(listaResultados, r => r.GramosConsumidos == 120.0m);
    }

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsRegistrosInRange()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var alimento = await DatabaseHelper.CreateTestAlimentoAsync("Arroz", 73, 28.0m);
        
        var fechaInicio = DateTime.Today.AddDays(-5);
        var fechaFin = DateTime.Today.AddDays(-1);
        var fechaFuera = DateTime.Today.AddDays(-10);
        
        // Registro dentro del rango
        var registro1 = new RegistroComida
        {
            AlimentoId = alimento.Id,
            FechaHora = fechaInicio.AddDays(2),
            GramosConsumidos = 100.0m,
            TipoComida = TipoComida.Almuerzo
        };
        await _registroRepository.AddAsync(registro1);
        
        // Registro dentro del rango
        var registro2 = new RegistroComida
        {
            AlimentoId = alimento.Id,
            FechaHora = fechaFin,
            GramosConsumidos = 150.0m,
            TipoComida = TipoComida.Cena
        };
        await _registroRepository.AddAsync(registro2);
        
        // Registro fuera del rango
        var registro3 = new RegistroComida
        {
            AlimentoId = alimento.Id,
            FechaHora = fechaFuera,
            GramosConsumidos = 80.0m,
            TipoComida = TipoComida.Desayuno
        };
        await _registroRepository.AddAsync(registro3);

        // Act
        var resultados = await _registroRepository.GetByDateRangeAsync(fechaInicio, fechaFin);
        var listaResultados = resultados.ToList();

        // Assert
        Assert.NotNull(listaResultados);
        Assert.True(listaResultados.Count >= 2);
        Assert.All(listaResultados, r => 
        {
            Assert.True(r.FechaHora.Date >= fechaInicio.Date);
            Assert.True(r.FechaHora.Date <= fechaFin.Date);
        });
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var alimento = await DatabaseHelper.CreateTestAlimentoAsync("Manzana", 38, 14.5m);
        
        var registro = new RegistroComida
        {
            AlimentoId = alimento.Id,
            FechaHora = DateTime.Now,
            GramosConsumidos = 150.0m,
            TipoComida = TipoComida.Desayuno
        };
        var registroCreado = await _registroRepository.AddAsync(registro);

        // Act
        var resultado = await _registroRepository.DeleteAsync(registroCreado.Id);

        // Assert
        Assert.True(resultado);
        
        // Verificar que ya no existe
        var existe = await DatabaseHelper.RegistroExistsAsync(registroCreado.Id);
        Assert.False(existe);
        
        // Verificar que GetByIdAsync retorna null
        var registroEliminado = await _registroRepository.GetByIdAsync(registroCreado.Id);
        Assert.Null(registroEliminado);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var idInexistente = 99999;

        // Act
        var resultado = await _registroRepository.DeleteAsync(idInexistente);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public async Task GetByDateAsync_WithNoRegistros_ReturnsEmptyList()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var fechaFutura = DateTime.Today.AddDays(30);

        // Act
        var resultados = await _registroRepository.GetByDateAsync(fechaFutura);
        var listaResultados = resultados.ToList();

        // Assert
        Assert.NotNull(listaResultados);
        Assert.Empty(listaResultados);
    }

    [Fact]
    public async Task AddAsync_WithDifferentTiposComida_CreatesCorrectly()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var alimento = await DatabaseHelper.CreateTestAlimentoAsync("Alimento Test", 50, 20.0m);
        
        var tiposComida = new[] 
        { 
            TipoComida.Desayuno, 
            TipoComida.Almuerzo, 
            TipoComida.Cena, 
            TipoComida.Snack 
        };

        // Act & Assert
        foreach (var tipoComida in tiposComida)
        {
            var registro = new RegistroComida
            {
                AlimentoId = alimento.Id,
                FechaHora = DateTime.Now,
                GramosConsumidos = 100.0m,
                TipoComida = tipoComida
            };
            
            var resultado = await _registroRepository.AddAsync(registro);
            
            Assert.NotNull(resultado);
            Assert.Equal(tipoComida, resultado.TipoComida);
            
            // Verificar que se guardó correctamente
            var registroVerificado = await _registroRepository.GetByIdAsync(resultado.Id);
            Assert.NotNull(registroVerificado);
            Assert.Equal(tipoComida, registroVerificado.TipoComida);
        }
    }
}

