using App_Indice_Glucemico.Server.Repositories;
using App_Indice_Glucemico.Shared;
using App_Indice_Glucemico.Tests.Helpers;
using Microsoft.Extensions.Configuration;

namespace App_Indice_Glucemico.Tests.Repositories;

/// <summary>
/// Tests de integración para UsuarioRepository
/// Estos tests se ejecutan contra la base de datos real
/// </summary>
[Collection("Database")]
public class UsuarioRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly IConfiguration _configuration;
    private readonly UsuarioRepository _repository;

    public UsuarioRepositoryTests(DatabaseFixture fixture)
    {
        _configuration = fixture.Configuration;
        _repository = new UsuarioRepository(_configuration);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsUsuario()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var usuarioTest = await DatabaseHelper.CreateTestUsuarioAsync(
            nombre: "Juan Pérez",
            email: "juan@test.com",
            password: "password123");

        // Act
        var resultado = await _repository.GetByIdAsync(usuarioTest.Id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(usuarioTest.Id, resultado.Id);
        Assert.Equal("Juan Pérez", resultado.Nombre);
        Assert.Equal("juan@test.com", resultado.Email);
        Assert.Equal("password123", resultado.Password);
        Assert.True(resultado.Activo);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();

        // Act
        var resultado = await _repository.GetByIdAsync(999);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ReturnsUsuario()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var usuarioTest = await DatabaseHelper.CreateTestUsuarioAsync(
            nombre: "María García",
            email: "maria@test.com",
            password: "password456");

        // Act
        var resultado = await _repository.GetByEmailAsync("maria@test.com");

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(usuarioTest.Id, resultado.Id);
        Assert.Equal("María García", resultado.Nombre);
        Assert.Equal("maria@test.com", resultado.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_WithInvalidEmail_ReturnsNull()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();

        // Act
        var resultado = await _repository.GetByEmailAsync("noexiste@test.com");

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsuarios()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var usuario1 = await DatabaseHelper.CreateTestUsuarioAsync(
            nombre: "Usuario 1",
            email: "usuario1@test.com");
        var usuario2 = await DatabaseHelper.CreateTestUsuarioAsync(
            nombre: "Usuario 2",
            email: "usuario2@test.com");

        // Act
        var resultados = (await _repository.GetAllAsync()).ToList();

        // Assert
        Assert.NotNull(resultados);
        Assert.True(resultados.Count >= 2);
        Assert.Contains(resultados, u => u.Id == usuario1.Id);
        Assert.Contains(resultados, u => u.Id == usuario2.Id);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithExistingEmail_ReturnsTrue()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var usuarioTest = await DatabaseHelper.CreateTestUsuarioAsync(
            email: "existente@test.com");

        // Act
        var resultado = await _repository.ExistsByEmailAsync("existente@test.com");

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithNonExistingEmail_ReturnsFalse()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();

        // Act
        var resultado = await _repository.ExistsByEmailAsync("noexiste@test.com");

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public async Task AddAsync_CreatesNewUsuario()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var nuevoUsuario = new Usuario
        {
            Nombre = "Nuevo Usuario",
            Email = "nuevo@test.com",
            Password = "password789",
            FechaCreacion = DateTime.Now,
            Activo = true
        };

        // Act
        var resultado = await _repository.AddAsync(nuevoUsuario);

        // Assert
        Assert.NotNull(resultado);
        Assert.True(resultado.Id > 0);
        Assert.Equal("Nuevo Usuario", resultado.Nombre);
        Assert.Equal("nuevo@test.com", resultado.Email);
        Assert.Equal("password789", resultado.Password);
        Assert.True(resultado.Activo);

        // Verificar que se guardó en la base de datos
        var existe = await DatabaseHelper.UsuarioExistsAsync(resultado.Id);
        Assert.True(existe);
    }

    [Fact]
    public async Task AddAsync_SetsFechaCreacion_WhenNotProvided()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var nuevoUsuario = new Usuario
        {
            Nombre = "Usuario Sin Fecha",
            Email = "sinfecha@test.com",
            Password = "password123",
            FechaCreacion = default,
            Activo = true
        };

        // Act
        var resultado = await _repository.AddAsync(nuevoUsuario);

        // Assert
        Assert.NotNull(resultado);
        Assert.NotEqual(default, resultado.FechaCreacion);
        Assert.True(resultado.FechaCreacion <= DateTime.Now);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingUsuario()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var usuarioTest = await DatabaseHelper.CreateTestUsuarioAsync(
            nombre: "Usuario Original",
            email: "original@test.com",
            password: "password123");

        var usuarioActualizado = new Usuario
        {
            Id = usuarioTest.Id,
            Nombre = "Usuario Actualizado",
            Email = "actualizado@test.com",
            Password = "nuevapassword",
            Activo = false
        };

        // Act
        var resultado = await _repository.UpdateAsync(usuarioActualizado);

        // Assert
        Assert.True(resultado);

        // Verificar que se actualizó correctamente
        var usuarioVerificado = await _repository.GetByIdAsync(usuarioTest.Id);
        Assert.NotNull(usuarioVerificado);
        Assert.Equal("Usuario Actualizado", usuarioVerificado.Nombre);
        Assert.Equal("actualizado@test.com", usuarioVerificado.Email);
        Assert.Equal("nuevapassword", usuarioVerificado.Password);
        Assert.False(usuarioVerificado.Activo);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ReturnsFalse()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var usuarioInexistente = new Usuario
        {
            Id = 999,
            Nombre = "Usuario Inexistente",
            Email = "inexistente@test.com",
            Password = "password123",
            Activo = true
        };

        // Act
        var resultado = await _repository.UpdateAsync(usuarioInexistente);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public async Task DeleteAsync_DeletesExistingUsuario()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var usuarioTest = await DatabaseHelper.CreateTestUsuarioAsync(
            nombre: "Usuario a Eliminar",
            email: "eliminar@test.com");

        // Act
        var resultado = await _repository.DeleteAsync(usuarioTest.Id);

        // Assert
        Assert.True(resultado);

        // Verificar que se eliminó
        var existe = await DatabaseHelper.UsuarioExistsAsync(usuarioTest.Id);
        Assert.False(existe);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ReturnsFalse()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();

        // Act
        var resultado = await _repository.DeleteAsync(999);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ReturnsUsuario()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var usuarioTest = await DatabaseHelper.CreateTestUsuarioAsync(
            nombre: "Usuario Activo",
            email: "activo@test.com",
            password: "password123",
            activo: true);

        // Act
        var resultado = await _repository.AuthenticateAsync("activo@test.com", "password123");

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(usuarioTest.Id, resultado.Id);
        Assert.Equal("activo@test.com", resultado.Email);
        Assert.True(resultado.Activo);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var usuarioTest = await DatabaseHelper.CreateTestUsuarioAsync(
            email: "test@test.com",
            password: "password123",
            activo: true);

        // Act
        var resultado = await _repository.AuthenticateAsync("test@test.com", "passwordincorrecto");

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInactiveUsuario_ReturnsNull()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();
        var usuarioTest = await DatabaseHelper.CreateTestUsuarioAsync(
            email: "inactivo@test.com",
            password: "password123",
            activo: false);

        // Act
        var resultado = await _repository.AuthenticateAsync("inactivo@test.com", "password123");

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task AuthenticateAsync_WithNonExistingEmail_ReturnsNull()
    {
        // Arrange
        await DatabaseHelper.CleanDatabaseAsync();

        // Act
        var resultado = await _repository.AuthenticateAsync("noexiste@test.com", "password123");

        // Assert
        Assert.Null(resultado);
    }
}

