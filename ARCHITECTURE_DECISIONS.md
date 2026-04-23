# Por qué tomamos estas decisiones (ADR)

Para que no te rompas la cabeza pensando por qué el código está así, acá te dejo las razones de las decisiones más importantes que tomamos.

## ADR-001: Clean Architecture
Separamos todo en 4 capas (Domain, Application, Infrastructure, WebAPI). La idea es que si mañana queremos cambiar SQL Server por otra base de datos, el código del negocio (Domain/Application) no se entere.

## ADR-002: CQRS con MediatR
Dividimos las acciones en Comandos (los que cambian datos) y Consultas (los que solo leen). Esto hace que el código no sea un espagueti de lógica metida en los controladores.

## ADR-003: EF Core + Dapper
- **EF Core:** Lo usamos para guardar datos y manejar las tablas. Es muy cómodo para eso.
- **Dapper:** Lo usamos solo para las lecturas pesadas o cuando necesitamos mucha velocidad, como validar si una sesión es válida en cada request.

## ADR-004: Multi-Tenancy por ID
Decidimos que cada tabla tenga un `TenantId`. Es la forma más segura de que un cliente no termine viendo los datos de otro por error.

## ADR-005: Auto-Healing de la DB
En desarrollo, a veces metemos mano en las tablas y las migraciones se rompen. Hicimos que el sistema, si detecta que la base de datos está corrupta o vieja, la borre y la vuelva a crear solita con los datos de prueba.

## ADR-006: Sesión por dispositivo
Por seguridad, solo dejamos una sesión activa por usuario. Si entras desde otro lado, el sistema te cierra la anterior automáticamente.

Última actualización: 23 de Marzo, 2026.
