namespace App_Indice_Glucemico.Tests.Repositories;

/// <summary>
/// Colección de tests para ejecutar los tests de repositorio de forma secuencial
/// Esto evita problemas de concurrencia en la base de datos
/// </summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // Esta clase solo se usa para definir la colección
    // No necesita código adicional
}

