using Microsoft.Extensions.Configuration;

namespace App_Indice_Glucemico.Tests.Helpers;

/// <summary>
/// Clase helper para cargar la configuración de tests desde appsettings.json
/// </summary>
public static class TestConfiguration
{
    private static IConfiguration? _configuration;

    /// <summary>
    /// Obtiene la configuración de tests
    /// </summary>
    public static IConfiguration GetConfiguration()
    {
        if (_configuration == null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        return _configuration;
    }

    /// <summary>
    /// Obtiene la cadena de conexión de tests
    /// </summary>
    public static string GetConnectionString()
    {
        var config = GetConfiguration();
        return config.GetConnectionString("app_indice_glucemico") 
            ?? throw new InvalidOperationException("Connection string 'app_indice_glucemico' not found in appsettings.json");
    }
}

