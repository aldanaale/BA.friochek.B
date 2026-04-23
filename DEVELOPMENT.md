# Guía para meterle mano al código

Si vas a programar algo nuevo acá, tratá de seguir estas reglas para que no se nos desordene el boliche.

## Convenciones de nombres

Tratamos de ser bien claros con los nombres:
- **Interfaces:** Siempre arrancan con `I` (ej: `IStoreRepository`).
- **Métodos asíncronos:** Tienen que terminar en `Async` (ej: `GetByIdAsync`).
- **Controladores:** Siempre en plural (ej: `StoresController`).

## Cómo agregar una funcionalidad nueva

Si tenés que hacer, por ejemplo, el CRUD de productos:

1. **Domain:** Crea la entidad `Product` y la interfaz `IProductRepository`.
2. **Infrastructure:** Implementá el repositorio y agregalo al `ApplicationDbContext`.
3. **Application:** Crea los DTOs, los Comandos (Create/Update) y las Consultas (GetAll/GetById).
4. **WebAPI:** Crea el controlador y llamá al `Mediator` para ejecutar la lógica.

## Manejo de Nulos

Usamos los "nullable reference types" de C#. Si algo puede ser nulo, poné el signo de pregunta (ej: `string? Phone`). Si es obligatorio, usá `= null!;` para que el compilador no te moleste.

## Base de Datos

- Si hacés cambios en las entidades, acordate de crear la migración:
  `dotnet ef migrations add NombreDeTuCambio --project src/BA.Backend.Infrastructure --startup-project src/BA.Backend.WebAPI`
- Si rompés algo en local, no pasa nada. Al reiniciar la app, el `DbInitializer` debería arreglarte el esquema (siempre que estés en desarrollo).

## Debugging

Si querés ver qué consultas SQL está tirando EF Core, fijate en la consola. Lo tenemos configurado en nivel `Debug` para que veas todo lo que pasa por atrás.

Última actualización: 23 de Marzo, 2026.
