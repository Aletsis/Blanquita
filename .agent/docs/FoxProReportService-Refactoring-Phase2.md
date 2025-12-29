# FoxProReportService - Fase 2: Servicios Especializados + CQRS

## Objetivo
Dividir `FoxProReportService` en servicios especializados siguiendo Clean Architecture, DDD y el patrón CQRS.

## Arquitectura Propuesta

### 1. Capa de Dominio (Domain)
- ✅ Ya existe: `SeriesDocumentoSucursal` (Value Object)
- 🆕 Agregar excepciones de dominio

### 2. Capa de Aplicación (Application)

#### 2.1 Queries (Lectura)
```
Application/
├── Queries/
│   ├── FoxPro/
│   │   ├── GetProductByCode/
│   │   │   ├── GetProductByCodeQuery.cs
│   │   │   └── GetProductByCodeQueryHandler.cs
│   │   ├── GetDocumentsByDateAndBranch/
│   │   │   ├── GetDocumentsByDateAndBranchQuery.cs
│   │   │   └── GetDocumentsByDateAndBranchQueryHandler.cs
│   │   ├── GetDailyCashCuts/
│   │   │   ├── GetDailyCashCutsQuery.cs
│   │   │   └── GetDailyCashCutsQueryHandler.cs
│   │   └── DiagnoseFoxProFile/
│   │       ├── DiagnoseFoxProFileQuery.cs
│   │       └── DiagnoseFoxProFileQueryHandler.cs
```

#### 2.2 Interfaces de Repositorios
```
Application/
├── Interfaces/
│   ├── Repositories/
│   │   ├── IFoxProProductRepository.cs
│   │   ├── IFoxProDocumentRepository.cs
│   │   ├── IFoxProCashCutRepository.cs
│   │   ├── IFoxProCashRegisterRepository.cs
│   │   └── IFoxProDiagnosticService.cs
```

### 3. Capa de Infraestructura (Infrastructure)

#### 3.1 Repositorios Especializados
```
Infrastructure/
├── ExternalServices/
│   ├── FoxPro/
│   │   ├── Repositories/
│   │   │   ├── FoxProProductRepository.cs
│   │   │   ├── FoxProDocumentRepository.cs
│   │   │   ├── FoxProCashCutRepository.cs
│   │   │   └── FoxProCashRegisterRepository.cs
│   │   ├── Services/
│   │   │   └── FoxProDiagnosticService.cs
│   │   ├── Common/
│   │   │   ├── DbfReaderFactory.cs
│   │   │   ├── DbfReaderExtensions.cs
│   │   │   └── FoxProSettings.cs
│   │   └── Mappers/
│   │       ├── FoxProProductMapper.cs
│   │       ├── FoxProDocumentMapper.cs
│   │       └── FoxProCashCutMapper.cs
```

## Plan de Implementación

### Paso 1: Crear Excepciones de Dominio
- [x] `FoxProConnectionException`
- [x] `FoxProDataReadException`
- [x] `FoxProFileNotFoundException`

### Paso 2: Crear Interfaces de Repositorios (Application)
- [x] `IFoxProProductRepository`
- [x] `IFoxProDocumentRepository`
- [x] `IFoxProCashCutRepository`
- [x] `IFoxProCashRegisterRepository`
- [x] `IFoxProDiagnosticService`

### Paso 3: Crear Queries y Handlers (Application)
- [x] `GetProductByCodeQuery` + Handler
- [x] `GetDocumentsByDateAndBranchQuery` + Handler
- [x] `GetDailyCashCutsQuery` + Handler
- [x] `DiagnoseFoxProFileQuery` + Handler

### Paso 4: Crear Infraestructura Común (Infrastructure)
- [x] `FoxProSettings` (Options Pattern)
- [x] `DbfReaderFactory`
- [x] `DbfReaderExtensions`

### Paso 5: Implementar Repositorios (Infrastructure)
- [x] `FoxProProductRepository`
- [x] `FoxProDocumentRepository`
- [x] `FoxProCashCutRepository`
- [x] `FoxProCashRegisterRepository`

### Paso 6: Implementar Mappers (Infrastructure)
- [x] `FoxProProductMapper`
- [x] `FoxProDocumentMapper`
- [x] `FoxProCashCutMapper`

### Paso 7: Implementar Servicios (Infrastructure)
- [x] `FoxProDiagnosticService`

### Paso 8: Actualizar Dependency Injection
- [x] Registrar repositorios
- [x] Registrar handlers (MediatR)
- [x] Registrar mappers
- [x] Configurar `FoxProSettings`
- [x] Agregar MediatR al proyecto Application
- [x] Registrar AddApplication() en Program.cs

### Paso 9: Migrar Código Existente
- [x] Deprecar `FoxProReportService`
- [x] Actualizar componentes Razor para usar handlers
- [x] Eliminar código obsoleto (deprecado, no eliminado)

### Paso 10: Testing
- [ ] Tests unitarios para handlers
- [ ] Tests unitarios para repositorios
- [ ] Tests de integración

## Beneficios Esperados

1. **Single Responsibility**: Cada repositorio tiene una responsabilidad única
2. **Testabilidad**: Handlers y repositorios pueden testearse independientemente
3. **Mantenibilidad**: Código más organizado y fácil de mantener
4. **Extensibilidad**: Fácil agregar nuevas queries/commands
5. **Desacoplamiento**: Componentes Razor no dependen de servicios de infraestructura
6. **Performance**: Posibilidad de cachear queries específicas
7. **CQRS**: Separación clara entre lectura y escritura (si se necesita en el futuro)
