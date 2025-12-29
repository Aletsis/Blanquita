# Blanquita

Sistema de gestión para la carnicería Blanquita, desarrollado con **.NET 8** y **Blazor Server**, siguiendo principios de **Clean Architecture** y **Domain-Driven Design (DDD)**.

## 📋 Tabla de Contenidos
- [Requisitos Previos](#requisitos-previos)
- [Arquitectura](#arquitectura)
- [Configuración y Ejecución Local](#configuración-y-ejecución-local)
- [Despliegue](#despliegue)
- [Documentación Adicional](#documentación-adicional)

## 🛠 Requisitos Previos

- **.NET 8.0 SDK** o superior.
- **SQL Server** (LocalDB, Express o Enterprise).
- **Visual Studio 2022** (versión 17.8 o superior recomendada) o **VS Code**.

## 🏗 Arquitectura

La solución sigue una estructura de Arquitectura Limpia dividida en las siguientes capas:

- **src/Blanquita.Domain**: Núcleo del negocio. Contiene Entidades, Value Objects, Interfaces de Repositorio y Eventos de Dominio. No tiene dependencias externas.
- **src/Blanquita.Application**: Lógica de aplicación, casos de uso, DTOs, validaciones e interfaces de servicios.
- **src/Blanquita.Infrastructure**: Implementación de repositorios, acceso a datos (EF Core), servicios externos (impresión, archivos DBF) y configuraciones concretas.
- **src/Blanquita.Web**: Capa de presentación (UI) construida con Blazor Server. Contiene Componentes, Páginas y Controladores.

## 🚀 Configuración y Ejecución Local

1. **Clonar el repositorio**
   ```bash
   git clone <https://github.com/Aletsis/Blanquita.git>
   cd Blanquita
   ```

2. **Configurar Base de Datos**
   Actualice la cadena de conexión en `src/Blanquita.Web/appsettings.json` o utilice **User Secrets** (recomendado para desarrollo).

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=BlanquitaDB;User Id=sa;Password=tu_password;TrustServerCertificate=True;"
   }
   ```

3. **Restaurar Dependencias**
   ```bash
   dotnet restore
   ```

4. **Ejecutar Migraciones (EF Core)**
   Si utiliza Entity Framework Core Code-First:
   ```bash
   cd src/Blanquita.Web
   dotnet ef database update
   ```

5. **Iniciar la Aplicación**
   ```bash
   dotnet run --project src/Blanquita.Web
   ```
   La aplicación estará disponible típicamente en `https://localhost:7001` o `http://localhost:5001`.

## 🌐 Despliegue

Para instrucciones detalladas sobre cómo desplegar esta aplicación en un servidor de producción con **IIS (Internet Information Services)**, consulte la guía dedicada:

👉 **[Guía de Despliegue en IIS](Docs/DEPLOY_IIS.md)**

## 📚 Documentación Adicional

En la carpeta `Docs/` encontrará documentación técnica detallada sobre cambios recientes y refactorizaciones:

- **[Cambios en Logging](Docs/CAMBIOS_LOGGING.md)**: Configuración de Serilog y sinks.
- **[Migración de Servicios Externos](Docs/EXTERNAL_SERVICES_MIGRATION.md)**: Refactorización de servicios de infraestructura.
- **[Refactorización de Configuración](Docs/REFACTORIZACION_CONFIGURACION.md)**: Cambios en el manejo de configuraciones globales.
- **[Guía Rápida DBF](Docs/GUIA_RAPIDA_DBF.md)**: Integración con sistemas legacy vía archivos DBF.
