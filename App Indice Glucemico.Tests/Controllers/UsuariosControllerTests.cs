using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using App_Indice_Glucemico.Server.Controllers;
using App_Indice_Glucemico.Shared;

namespace App_Indice_Glucemico.Tests.Controllers;

/// <summary>
/// Tests unitarios para UsuariosController
/// </summary>
public class UsuariosControllerTests
{
    private readonly Mock<IUsuarioRepository> _mockRepository;
    private readonly Mock<ILogger<UsuariosController>> _mockLogger;
    private readonly UsuariosController _controller;

    public UsuariosControllerTests()
    {
        _mockRepository = new Mock<IUsuarioRepository>();
        _mockLogger = new Mock<ILogger<UsuariosController>>();
        _controller = new UsuariosController(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithListOfUsuarios()
    {
        // Arrange
        var usuarios = new List<Usuario>
        {
            new Usuario { Id = 1, Nombre = "Juan Pérez", Email = "juan@test.com", FechaCreacion = DateTime.Now, Activo = true },
            new Usuario { Id = 2, Nombre = "María García", Email = "maria@test.com", FechaCreacion = DateTime.Now, Activo = true }
        };

        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(usuarios);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        var usuariosList = returnValue.ToList();
        Assert.Equal(2, usuariosList.Count);
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
    public async Task GetById_ReturnsOkResult_WithUsuario()
    {
        // Arrange
        var usuario = new Usuario 
        { 
            Id = 1, 
            Nombre = "Juan Pérez", 
            Email = "juan@test.com",
            FechaCreacion = DateTime.Now,
            Activo = true
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenUsuarioDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Usuario?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Contains("999", notFoundResult.Value?.ToString() ?? "");
    }

    [Fact]
    public async Task GetById_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetByEmail_ReturnsOkResult_WithUsuario()
    {
        // Arrange
        var usuario = new Usuario 
        { 
            Id = 1, 
            Nombre = "Juan Pérez", 
            Email = "juan@test.com",
            FechaCreacion = DateTime.Now,
            Activo = true
        };

        _mockRepository.Setup(r => r.GetByEmailAsync("juan@test.com")).ReturnsAsync(usuario);

        // Act
        var result = await _controller.GetByEmail("juan@test.com");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetByEmail_ReturnsNotFound_WhenUsuarioDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByEmailAsync("noexiste@test.com")).ReturnsAsync((Usuario?)null);

        // Act
        var result = await _controller.GetByEmail("noexiste@test.com");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Contains("noexiste@test.com", notFoundResult.Value?.ToString() ?? "");
    }

    [Fact]
    public async Task GetByEmail_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByEmailAsync("test@test.com")).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetByEmail("test@test.com");

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreatedResult_WithUsuario()
    {
        // Arrange
        var nuevoUsuario = new Usuario
        {
            Nombre = "Nuevo Usuario",
            Email = "nuevo@test.com",
            Password = "password123",
            FechaCreacion = DateTime.Now,
            Activo = true
        };

        var usuarioCreado = new Usuario
        {
            Id = 1,
            Nombre = "Nuevo Usuario",
            Email = "nuevo@test.com",
            Password = "password123",
            FechaCreacion = DateTime.Now,
            Activo = true
        };

        _mockRepository.Setup(r => r.ExistsByEmailAsync("nuevo@test.com")).ReturnsAsync(false);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Usuario>())).ReturnsAsync(usuarioCreado);

        // Act
        var result = await _controller.Create(nuevoUsuario);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.NotNull(createdResult.Value);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenUsuarioIsNull()
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
        var nuevoUsuario = new Usuario
        {
            Nombre = "",
            Email = "test@test.com",
            Password = "password123"
        };

        // Act
        var result = await _controller.Create(nuevoUsuario);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenEmailIsEmpty()
    {
        // Arrange
        var nuevoUsuario = new Usuario
        {
            Nombre = "Test User",
            Email = "",
            Password = "password123"
        };

        // Act
        var result = await _controller.Create(nuevoUsuario);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenEmailFormatIsInvalid()
    {
        // Arrange
        var nuevoUsuario = new Usuario
        {
            Nombre = "Test User",
            Email = "emailinvalido",
            Password = "password123"
        };

        // Act
        var result = await _controller.Create(nuevoUsuario);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenPasswordIsEmpty()
    {
        // Arrange
        var nuevoUsuario = new Usuario
        {
            Nombre = "Test User",
            Email = "test@test.com",
            Password = ""
        };

        // Act
        var result = await _controller.Create(nuevoUsuario);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenPasswordIsTooShort()
    {
        // Arrange
        var nuevoUsuario = new Usuario
        {
            Nombre = "Test User",
            Email = "test@test.com",
            Password = "12345" // Menos de 6 caracteres
        };

        // Act
        var result = await _controller.Create(nuevoUsuario);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        var nuevoUsuario = new Usuario
        {
            Nombre = "Test User",
            Email = "existente@test.com",
            Password = "password123"
        };

        _mockRepository.Setup(r => r.ExistsByEmailAsync("existente@test.com")).ReturnsAsync(true);

        // Act
        var result = await _controller.Create(nuevoUsuario);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Contains("existente@test.com", conflictResult.Value?.ToString() ?? "");
    }

    [Fact]
    public async Task Create_SetsFechaCreacion_WhenNotProvided()
    {
        // Arrange
        var nuevoUsuario = new Usuario
        {
            Nombre = "Test User",
            Email = "test@test.com",
            Password = "password123",
            FechaCreacion = default
        };

        var usuarioCreado = new Usuario
        {
            Id = 1,
            Nombre = "Test User",
            Email = "test@test.com",
            Password = "password123",
            FechaCreacion = DateTime.Now,
            Activo = true
        };

        _mockRepository.Setup(r => r.ExistsByEmailAsync("test@test.com")).ReturnsAsync(false);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Usuario>())).ReturnsAsync(usuarioCreado);

        // Act
        var result = await _controller.Create(nuevoUsuario);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        _mockRepository.Verify(r => r.AddAsync(It.Is<Usuario>(u => u.FechaCreacion != default)), Times.Once);
    }

    [Fact]
    public async Task Create_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var nuevoUsuario = new Usuario
        {
            Nombre = "Test User",
            Email = "test@test.com",
            Password = "password123"
        };

        _mockRepository.Setup(r => r.ExistsByEmailAsync("test@test.com")).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Create(nuevoUsuario);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenUpdateSucceeds()
    {
        // Arrange
        var usuarioExistente = new Usuario
        {
            Id = 1,
            Nombre = "Usuario Original",
            Email = "original@test.com",
            Password = "password123",
            Activo = true
        };

        var usuarioActualizado = new Usuario
        {
            Id = 1,
            Nombre = "Usuario Actualizado",
            Email = "actualizado@test.com",
            Password = "nuevapassword",
            Activo = false
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuarioExistente);
        _mockRepository.Setup(r => r.GetByEmailAsync("actualizado@test.com")).ReturnsAsync((Usuario?)null);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Usuario>())).ReturnsAsync(true);

        // Act
        var result = await _controller.Update(1, usuarioActualizado);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenUsuarioIsNull()
    {
        // Act
        var result = await _controller.Update(1, null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenIdMismatch()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 2,
            Nombre = "Test User",
            Email = "test@test.com",
            Password = "password123"
        };

        // Act
        var result = await _controller.Update(1, usuario);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenNombreIsEmpty()
    {
        // Arrange
        var usuarioExistente = new Usuario { Id = 1, Nombre = "Test", Email = "test@test.com", Password = "pass" };
        var usuarioActualizado = new Usuario { Id = 1, Nombre = "", Email = "test@test.com", Password = "pass" };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuarioExistente);

        // Act
        var result = await _controller.Update(1, usuarioActualizado);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenEmailFormatIsInvalid()
    {
        // Arrange
        var usuarioExistente = new Usuario { Id = 1, Nombre = "Test", Email = "test@test.com", Password = "pass" };
        var usuarioActualizado = new Usuario { Id = 1, Nombre = "Test", Email = "emailinvalido", Password = "pass" };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuarioExistente);

        // Act
        var result = await _controller.Update(1, usuarioActualizado);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenUsuarioDoesNotExist()
    {
        // Arrange
        var usuarioActualizado = new Usuario
        {
            Id = 999,
            Nombre = "Test User",
            Email = "test@test.com",
            Password = "password123"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Usuario?)null);

        // Act
        var result = await _controller.Update(999, usuarioActualizado);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsConflict_WhenEmailIsUsedByAnotherUsuario()
    {
        // Arrange
        var usuarioExistente = new Usuario
        {
            Id = 1,
            Nombre = "Usuario 1",
            Email = "usuario1@test.com",
            Password = "password123"
        };

        var otroUsuario = new Usuario
        {
            Id = 2,
            Nombre = "Usuario 2",
            Email = "usuario2@test.com",
            Password = "password123"
        };

        var usuarioActualizado = new Usuario
        {
            Id = 1,
            Nombre = "Usuario 1",
            Email = "usuario2@test.com", // Email de otro usuario
            Password = "password123"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuarioExistente);
        _mockRepository.Setup(r => r.GetByEmailAsync("usuario2@test.com")).ReturnsAsync(otroUsuario);

        // Act
        var result = await _controller.Update(1, usuarioActualizado);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Update_KeepsPassword_WhenPasswordIsEmpty()
    {
        // Arrange
        var usuarioExistente = new Usuario
        {
            Id = 1,
            Nombre = "Test User",
            Email = "test@test.com",
            Password = "passwordoriginal",
            Activo = true
        };

        var usuarioActualizado = new Usuario
        {
            Id = 1,
            Nombre = "Test User Actualizado",
            Email = "test@test.com",
            Password = "", // Password vacío
            Activo = true
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuarioExistente);
        _mockRepository.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(usuarioExistente);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Usuario>())).ReturnsAsync(true);

        // Act
        var result = await _controller.Update(1, usuarioActualizado);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Usuario>(u => u.Password == "passwordoriginal")), Times.Once);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenPasswordIsTooShort()
    {
        // Arrange
        var usuarioExistente = new Usuario { Id = 1, Nombre = "Test", Email = "test@test.com", Password = "pass" };
        var usuarioActualizado = new Usuario { Id = 1, Nombre = "Test", Email = "test@test.com", Password = "12345" };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuarioExistente);
        _mockRepository.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(usuarioExistente);

        // Act
        var result = await _controller.Update(1, usuarioActualizado);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var usuarioExistente = new Usuario { Id = 1, Nombre = "Test", Email = "test@test.com", Password = "pass" };
        var usuarioActualizado = new Usuario { Id = 1, Nombre = "Test", Email = "test@test.com", Password = "pass" };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Update(1, usuarioActualizado);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleteSucceeds()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 1,
            Nombre = "Usuario a Eliminar",
            Email = "eliminar@test.com",
            Password = "password123"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);
        _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenUsuarioDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Usuario?)null);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsInternalServerError_WhenDeleteFails()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 1,
            Nombre = "Test User",
            Email = "test@test.com",
            Password = "password123"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);
        _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task Authenticate_ReturnsOkResult_WithUsuario()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 1,
            Nombre = "Test User",
            Email = "test@test.com",
            Password = "password123",
            Activo = true
        };

        var loginRequest = new LoginRequest
        {
            Email = "test@test.com",
            Password = "password123"
        };

        _mockRepository.Setup(r => r.AuthenticateAsync("test@test.com", "password123")).ReturnsAsync(usuario);

        // Act
        var result = await _controller.Authenticate(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Authenticate_ReturnsUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@test.com",
            Password = "passwordincorrecto"
        };

        _mockRepository.Setup(r => r.AuthenticateAsync("test@test.com", "passwordincorrecto")).ReturnsAsync((Usuario?)null);

        // Act
        var result = await _controller.Authenticate(loginRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Authenticate_ReturnsBadRequest_WhenRequestIsNull()
    {
        // Act
        var result = await _controller.Authenticate(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Authenticate_ReturnsBadRequest_WhenEmailIsEmpty()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "",
            Password = "password123"
        };

        // Act
        var result = await _controller.Authenticate(loginRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Authenticate_ReturnsBadRequest_WhenPasswordIsEmpty()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@test.com",
            Password = ""
        };

        // Act
        var result = await _controller.Authenticate(loginRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Authenticate_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@test.com",
            Password = "password123"
        };

        _mockRepository.Setup(r => r.AuthenticateAsync("test@test.com", "password123")).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Authenticate(loginRequest);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}

