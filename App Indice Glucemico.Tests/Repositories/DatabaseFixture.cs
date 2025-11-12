using Microsoft.Extensions.Configuration;

namespace App_Indice_Glucemico.Tests.Repositories;

/// <summary>
/// Fixture para compartir la configuración entre tests
/// </summary>
public class DatabaseFixture : IDisposable
{
    public IConfiguration Configuration { get; }

    public DatabaseFixture()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        Configuration = builder.Build();
    }

    public void Dispose()
    {
        // Cleanup si es necesario
    }
}

