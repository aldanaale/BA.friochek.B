# BA Backend - Plataforma NFC Tracking

Este es el backend principal para la plataforma de gestión y 
trazabilidad de activos mediante NFC. Está diseñado para soportar 
múltiples empresas (multi-tenant) y servir tanto a la app móvil 
de terreno como al panel administrativo web.

---

## El Stack que usamos

Decidimos armar esto con tecnologías modernas y robustas para 
que sea fácil de escalar:

- **Framework:** .NET 10 (C#)
- **Arquitectura:** Clean Architecture con el patrón CQRS 
  (usamos MediatR para que el código esté bien ordenado).
- **Base de Datos:** SQL Server 2022.
- **Persistencia:** EF Core 10 para las operaciones normales 
  y Dapper 2.1 para cuando necesitamos consultas que vuelen 
  (como validar sesiones).
- **Seguridad:** JWT de 15 minutos para los tokens, BCrypt 
  (factor 12) para que las contraseñas estén seguras y un 
  sistema de huella digital (SHA-256) para controlar los 
  dispositivos.

---

## ¿Cómo lo pongo a correr?

Es bastante directo, solo sigue estos pasos:

1. **Requisitos:** Ten instalado el SDK de .NET 10 y SQL 
   Server 2022 Express.
2. **Base de datos:** Ejecuta los scripts en orden:
```
   sql/01_initial_schema.sql
   sql/02_add_password_reset_tokens.sql
   sql/03_add_business_tables.sql
```
3. **Configura** tu cadena de conexión en 
   `src/BA.Backend.WebAPI/appsettings.json`:
```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=TU_SERVIDOR\\SQLEXPRESS;
     Database=BA_Backend_DB;Trusted_Connection=True;
     TrustServerCertificate=True;"
   }
```
4. **Corre:** `dotnet run --project src/BA.Backend.WebAPI`

---

## Para las pruebas (Swagger)

Una vez que corra, entra a `http://localhost:5003/swagger`. 
Usa estas credenciales para probar el login:

- **Empresa (Tenant):** `admin`
- **Email:** `admin@test.com`
- **Pass:** `Admin123!`

---

## ¿Qué hay listo y qué falta?

A día de hoy, esto es lo que tenemos arriba:

- [x] **Autenticación:** Login seguro con JWT, sesión única por dispositivo y botón de pánico para revocar     accesos.
- [x] **Usuarios:** CRUD completo, solo el Admin puede crear.
- [x] **Tiendas (Stores):** Registro de locales con GPS.
- [x] **Cliente - Home:** Dashboard con estado de máquinas 
  y pedidos activos.
- [x] **Cliente - Pedidos:** Flujo completo NFC → catálogo. 
  → confirmar, con validación de capacidad del cooler.
- [x] **Cliente - Asistencia Técnica:** Solicitudes con fotos 
  y reporte de tag dañado.
- [x] **Validación NFC:** Endpoints para escanear tags.
- [x] **Transportista:** Rutas, entregas con validación 
  NFC y registro de mermas con foto obligatoria.
- [x] **Técnico:** Tickets, reparaciones y cierre con firma.

---

## Estructura de carpetas

Para que no te pierdas:
- `Domain`: Donde viven las reglas de negocio y las entidades.
- `Application`: Aquí están los comandos (Commands) y 
  consultas (Queries).
- `Infrastructure`: Todo lo que toca "el afuera" 
  (Base de datos, servicios externos).
- `WebAPI`: Los controladores y la configuración de la API.
- `sql/`: Scripts para crear la base de datos desde cero.

---

Cualquier duda, revisa la carpeta de documentación donde 
hay detalles de la arquitectura y ejemplos de la API.

**Última mano le metimos el:** 29 de Marzo, 2026.