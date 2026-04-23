# INFORME FINAL - BA.FrioCheck

**Fecha:** 2026-04-17 09:35:41
**Proyecto:** BA.FrioCheck (Antigravity) | Puerto 5003

---

## Resultados de Tests

| Metrica | Valor |
|---------|-------|
| Total tests | 218 |
| PASS | 212 |
| FAIL | 6 |
| SKIP | 0 |
| Health Score | **97 / 100** |

### Por bloque

| Bloque | PASS | FAIL | SKIP | % |
|--------|------|------|------|---|
| B1  - Conexion API | 7 |  | 0 | 100% PARCIAL |
| B2  - Auth/Login | 22 | 0 | 0 | 100% OK |
| B3  - ApiResponse | 10 | 0 | 0 | 100% OK |
| B4  - Roles | 17 |  | 0 | 100% PARCIAL |
| B5  - Multi-tenant | 5 | 0 | 0 | 100% OK |
| B6  - CRUD Usuarios | 17 | 0 | 0 | 100% OK |
| B7  - CRUD Stores | 11 | 0 | 0 | 100% OK |
| B8  - NFC | 5 | 0 | 0 | 100% OK |
| B9  - Cliente | 9 | 0 | 0 | 100% OK |
| B10 - Tecnico | 5 | 0 | 0 | 100% OK |
| B11 - Transportista |  | 4 | 0 | 0% FALLO |
| B12 - Dashboard | 3 | 0 | 0 | 100% OK |
| B13 - Base de Datos | 25 | 0 | 0 | 100% OK |
| B14 - JWT/Sesion | 6 | 0 | 0 | 100% OK |
| B15 - Exceptions | 7 | 0 | 0 | 100% OK |
| B16 - Endpoints | 8 | 0 | 0 | 100% OK |
| B17 - Coolers CRUD | 10 | 0 | 0 | 100% OK |
| B18 - Post-Limpieza | 10 | 0 | 0 | 100% OK |
| B19 - Mermas/TechSupp | 6 | 0 | 0 | 100% OK |
| B20 - Password Reset | 8 | 0 | 0 | 100% OK |
| B21 - Seguridad | 8 | 0 | 0 | 100% OK |
| B22 - Performance | 7 | 0 | 0 | 100% OK |
| B23 - Concurrencia | 5 | 0 | 0 | 100% OK |

### Tests fallidos

| ID | Test | Detalle |
|----|------|---------|
| `C07` | Header CORS presente con Origin cross-origin | CORS:  |
| `R14` | Cliente NO accede a GET /transportista/route (403) | Status:404 |
| `TR02` | GET /transportista/route retorna 200 (data puede ser null = BUG conocido) | Status:404 data=null (BUG: deberia ser []) |
| `TR03` | POST /transportista/delivery con nfcToken invalido retorna 401 o 400 | Status:404 |
| `TR04` | Cliente NO puede acceder a GET /transportista/route (403) | Status:404 |
| `TR05` | Tecnico NO puede registrar delivery (403) | Status:404 |

---

## Limpieza del Repositorio

| Metrica | Valor |
|---------|-------|
| Archivos/carpetas eliminados | 0 |
| Espacio liberado | 0 B |

---

## Hallazgos de Seguridad y Calidad

### ALTA (18 hallazgos)

| Tipo | Descripcion | Archivo |
|------|-------------|---------|
| SEGURIDAD | Clave JWT posiblemente corta para HS256 (necesita >= 32 chars) | `\src\BA.Backend.Infrastructure\Services\JwtTokenService.cs` |
| SEGURIDAD | Posible log de dato sensible linea 48 | `\src\BA.Backend.Application\Auth\Handlers\ForgotPasswordCommandHandler.cs` |
| SEGURIDAD | Posible log de dato sensible linea 69 | `\src\BA.Backend.Application\Auth\Handlers\ForgotPasswordCommandHandler.cs` |
| SEGURIDAD | Posible log de dato sensible linea 27 | `\src\BA.Backend.Infrastructure\Services\JwtTokenService.cs` |
| SEGURIDAD | Posible log de dato sensible linea 55 | `\src\BA.Backend.Infrastructure\Services\JwtTokenService.cs` |
| SEGURIDAD | Posible log de dato sensible linea 62 | `\src\BA.Backend.Infrastructure\Services\JwtTokenService.cs` |
| SEGURIDAD | Posible log de dato sensible linea 93 | `\src\BA.Backend.Infrastructure\Services\JwtTokenService.cs` |
| SEGURIDAD | Posible log de dato sensible linea 111 | `\src\BA.Backend.Infrastructure\Services\JwtTokenService.cs` |
| SEGURIDAD | Posible log de dato sensible linea 123 | `\src\BA.Backend.Infrastructure\Services\JwtTokenService.cs` |
| SEGURIDAD | Posible log de dato sensible linea 127 | `\src\BA.Backend.Infrastructure\Services\JwtTokenService.cs` |
| SEGURIDAD | Posible log de dato sensible linea 142 | `\src\BA.Backend.Infrastructure\Services\JwtTokenService.cs` |
| SEGURIDAD | Posible log de dato sensible linea 210 | `\src\BA.Backend.Infrastructure\Services\JwtTokenService.cs` |
| SEGURIDAD | Posible log de dato sensible linea 19 | `\src\BA.Backend.Infrastructure\Services\PasswordHasher.cs` |
| SEGURIDAD | Posible log de dato sensible linea 28 | `\src\BA.Backend.Infrastructure\Services\PasswordHasher.cs` |
| SEGURIDAD | Posible log de dato sensible linea 38 | `\src\BA.Backend.Infrastructure\Services\PasswordHasher.cs` |
| SEGURIDAD | Posible log de dato sensible linea 42 | `\src\BA.Backend.Infrastructure\Services\PasswordHasher.cs` |
| SEGURIDAD | Posible log de dato sensible linea 49 | `\src\BA.Backend.Infrastructure\Services\PasswordHasher.cs` |
| SEGURIDAD | Posible log de dato sensible linea 46 | `\src\BA.Backend.WebAPI\Middleware\SessionValidationMiddleware.cs` |

### MEDIA (16 hallazgos)

| Tipo | Descripcion | Archivo |
|------|-------------|---------|
| SEGURIDAD | Token JWT posiblemente guardado en texto plano | `\src\BA.Backend.WebAPI\Program.cs` |
| SEGURIDAD | DTO con campo sensible (Role/IsAdmin/TenantId) sin restriccion de binding | `\src\BA.Backend.Application\Supervisor\DTOs\SupervisorDashboardDto.cs` |
| SEGURIDAD | DTO con campo sensible (Role/IsAdmin/TenantId) sin restriccion de binding | `\src\BA.Backend.Application\Users\DTOs\CreateUserDto.cs` |
| SEGURIDAD | DTO con campo sensible (Role/IsAdmin/TenantId) sin restriccion de binding | `\src\BA.Backend.Application\Users\DTOs\UpdateUserDto.cs` |
| SEGURIDAD | DTO con campo sensible (Role/IsAdmin/TenantId) sin restriccion de binding | `\src\BA.Backend.Application\Users\DTOs\UserDto.cs` |
| SEGURIDAD | DTO con campo sensible (Role/IsAdmin/TenantId) sin restriccion de binding | `\src\BA.Backend.Domain\Entities\TechSupportRequest.cs` |
| SEGURIDAD | .gitignore no cubre: *.key | `.gitignore` |
| SEGURIDAD | .gitignore no cubre: *.pfx | `.gitignore` |
| SEGURIDAD | .gitignore no cubre: *.p12 | `.gitignore` |
| SEGURIDAD | .gitignore no cubre: appsettings.Local.json | `.gitignore` |
| SEGURIDAD | .gitignore no cubre: secrets.json | `.gitignore` |
| SEGURIDAD | UseHttpsRedirection no detectado en Program.cs | `\src\BA.Backend.WebAPI\Program.cs` |
| CALIDAD | Nombre duplicado 'ServiceCollectionExtensions.cs' | `\src\BA.Backend.Application\ServiceCollectionExtensions.cs | \src\BA.Backend.Infrastructure\ServiceCollectionExtensions.cs` |
| CALIDAD | Nombre duplicado 'ClienteRequests.cs' | `\src\BA.Backend.Application\Cliente\DTOs\ClienteRequests.cs | \src\BA.Backend.WebAPI\DTOs\Cliente\ClienteRequests.cs` |
| CALIDAD | Nombre duplicado 'NfcRequests.cs' | `\src\BA.Backend.Application\Nfc\DTOs\NfcRequests.cs | \src\BA.Backend.WebAPI\DTOs\Nfc\NfcRequests.cs` |
| CALIDAD | Nombre duplicado 'appsettings.json' | `\src\BA.Backend.WebAPI\appsettings.json | \tests\BA.Backend.Application.Tests\appsettings.json` |

### BAJA ( hallazgos)

| Tipo | Descripcion | Archivo |
|------|-------------|---------|
| LIMPIEZA | No se pudo eliminar bin - Acceso denegado a la ruta de acceso 'Microsoft.Data.SqlClient.resources.dll'. | - |

---

## Checklist de Acciones

- [ ] Corregir 6 tests fallidos
- [ ] Resolver 0 hallazgos CRITICOS de seguridad
- [ ] Resolver 18 hallazgos ALTOS
- [ ] Revisar hallazgos MEDIA y BAJA
- [ ] Instalar sqlcmd para habilitar tests de BD
- [ ] Agregar tests a pipeline CI/CD

---
*Generado automaticamente por test.ps1*

