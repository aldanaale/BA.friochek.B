# Cómo está organizado el proyecto

Acá te explico qué hay en cada carpeta para que no andes perdido buscando archivos.

## Estructura de la solución

- **src/BA.Backend.Domain**: Es el núcleo. Acá están las entidades (User, Store, Cooler, Order, TechSupportRequest) y las interfaces de los repositorios. No tiene dependencias de nada, es código puro de negocio.
- **src/BA.Backend.Application**: Acá vive la lógica de los casos de uso. Usamos CQRS, así que vas a ver carpetas de `Commands` (acciones) y `Queries` (lecturas). También están los DTOs para pasar datos al front.
- **src/BA.Backend.Infrastructure**: Acá está la implementación técnica. La conexión a SQL Server, EF Core, Dapper y los servicios externos como el de generar tokens o hashear passwords.
- **src/BA.Backend.WebAPI**: Es la cara visible. Los controladores que reciben las peticiones HTTP y la configuración de seguridad (JWT, CORS, etc.).

## Flujo de una petición

Para que entiendas el camino que hace un dato:
1. Llega al **Controller** (WebAPI).
2. Se manda un comando al **Mediator** (Application).
3. El **Behavior** lo valida (FluentValidation).
4. El **Handler** procesa la lógica y usa un **Repository** (Infrastructure).
5. El **Repository** guarda o lee de la base de datos.
6. El resultado vuelve formateado como **DTO**.

## Base de Datos

- **sql/**: Guardamos scripts manuales por si necesitás correr algo directo en el SSMS.
- **Migrations**: Están dentro de Infrastructure. Se aplican solas al arrancar la app.

Última actualización: 23 de Marzo, 2026.
