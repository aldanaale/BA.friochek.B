# DOCUMENTACIÓN TÉCNICA COMPLETA — BA.FrioCheck

Este es el manual exhaustivo del proyecto. Contiene la referencia granular de cada archivo, decisión de diseño y mecanismo interno.

---

## 1. Estructura del Dominio (`BA.Backend.Domain`)

### 🛡️ Entidades de Auditoría (`BaseEntity`)
Todas las entidades maestros heredan de `BaseEntity`, lo que garantiza:
- `CreatedAt` / `CreatedBy`: Registro de origen.
- `UpdatedAt` / `UpdatedBy`: Registro de modificación.
- `IsDeleted` / `DeletedAt`: Flag para **Soft Delete** (eliminación lógica).

### 🏢 Multi-Tenancy (`ITenantEntity`)
Implementa el campo `TenantId`. Durante el login, el sistema inyecta este ID en el JWT. El `ApplicationDbContext` aplica un **Global Query Filter** que añade automáticamente `WHERE TenantId = @currentTenant` a cada consulta.

### 📦 Diccionario de Entidades Clave
- **User**: Usuarios con `PasswordHash` (BCrypt) y roles.
- **Cooler**: Activos vinculados a una `Store`.
- **NfcTag**: Identificador físico con `SecurityHash`.
- **Order / OrderItem**: Gestión de pedidos con cálculo automático de totales.
- **Route / RouteStop**: Entidades raíz para la logística de entrega.
- **Product**: Catálogo con gestión de `Stock`.
- **Merma**: Registro de pérdidas con `PhotoUrl` obligatoria.

---

## 2. Capa de Aplicación (`BA.Backend.Application`)

### ⚡ Patrón CQRS con MediatR
Dividimos la responsabilidad en **Commands** (Cambio de estado) y **Queries** (Lectura de datos).

**Handlers Destacados**:
- **LoginCommandHandler**: Realiza una búsqueda global por email (ignorando filtros de tenant), identifica el inquilino, verifica la contraseña y genera el JWT + ID de sesión.
- **DeliveryCommandHandler**: Valida el token NFC, marca la entrega en la ruta y descuenta el stock del inventario.
- **DashboardHandlers**: Utilizan `IDashboardRepository` para disparar consultas optimizadas via Dapper.

### 🛡️ Validaciones (`FluentValidation`)
Cada request de escritura tiene un validador que se ejecuta antes del handler, previniendo estados inconsistentes (ej: contraseñas cortas, SKUs inexistentes).

---

## 3. Infraestructura (`BA.Backend.Infrastructure`)

### 🗄️ Persistencia Dual (EF Core + Dapper)
- **EF Core**: Encargado de las transacciones complejas y el mantenimiento de la integridad referencial.
- **Dapper**: Encargado de las validaciones de sesión en cada request y de la carga de dashboards masivos para una respuesta instantánea.

### 🔑 Servicios de Seguridad
- **JwtTokenService**: Genera tokens con claims de sesión, tenant y rol.
- **PasswordHasher**: Implementación de BCrypt con factor de costo 12.
- **SessionService**: Monitor de sesiones activas que permite el bloqueo de acceso en tiempo real si el usuario es desactivado.

---

## 4. WebAPI (`BA.Backend.WebAPI`)

### 🚀 Middleware Pipeline
1. **GlobalExceptionHandler**: Captura `DomainException` y retorna el código de error correspondiente.
2. **RateLimiting**: Protege el login contra ataques de fuerza bruta.
3. **Authentication/Authorization**: Valida el JWT y el rol del usuario.
4. **SessionValidation**: Verifica con Dapper si la sesión sigue activa.

---

## 5. Base de Datos SQL

- **Nombre**: `BD_FC`
- **Initial Seed**: Poblado mediante `02_seed_data.sql` con usuarios de prueba y datos determinísticos.
- **Migraciones**: Gestionadas con EF Core Migrations para versionamiento del esquema.
