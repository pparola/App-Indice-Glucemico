# Configuración de la Aplicación

## Configuración Inicial

Para que la aplicación funcione correctamente, necesitas crear tu propio archivo `appsettings.json` basándote en el archivo de ejemplo.

### Pasos:

1. **Copia el archivo de ejemplo:**
   ```bash
   cp appsettings.json.example appsettings.json
   ```

2. **Edita `appsettings.json`** y configura los siguientes valores:

   - **ConnectionStrings**: Configura la cadena de conexión a tu base de datos MySQL
   - **SmtpSettings**: Configura los datos de tu servidor SMTP para el envío de correos

### Ejemplo de configuración:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "SmtpSettings": {
    "Server": "smtp.gmail.com",
    "Port": "587",
    "Titulo": "Tu Nombre o Empresa",
    "UserName": "tu-email@gmail.com",
    "Password": "tu-contraseña-de-aplicacion"
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "app_indice_glucemico": "server=localhost;port=3306;uid=tu_usuario;database=app_indice_glucemico;password=tu_contraseña;sslmode=none"
  }
}
```

### Notas Importantes:

- **NUNCA** subas el archivo `appsettings.json` al repositorio, ya que contiene información sensible.
- El archivo `appsettings.json.example` es solo una plantilla y no contiene datos reales.
- Para Gmail, necesitarás generar una "Contraseña de aplicación" desde tu cuenta de Google.

