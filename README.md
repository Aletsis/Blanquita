# Blanquita

Proyecto Blazor para la gestión de la carnicería Blanquita.

## Requisitos previos

- .NET 8.0 SDK
- SQL Server

## Configuración

1.  Clonar el repositorio.
2.  Configurar la cadena de conexión en `appsettings.json` o usando User Secrets.
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Database=Pruebas;User Id=TU_USUARIO;Password=TU_CONTRASEÑA;TrustServerCertificate=True;MultipleActiveResultSets=True;"
    }
    ```
3.  Ejecutar las migraciones de base de datos (si aplica).
4.  Ejecutar el proyecto:
    ```bash
    dotnet run --project Blanquita
    ```

## Estructura del Proyecto

- **Blanquita**: Proyecto principal Blazor Server/WebAssembly.
