# Bitácora de Commits - Rama debug

Este archivo documenta los cambios realizados en la rama `debug`, la cual consolida las funcionalidades más recientes del proyecto.

## Historial de Cambios

### [6f2642f] - Merge main con modulos Tecnico y Transportista - Rama Debug
**Fecha:** 2026-03-25  
**Autor:** Trae AI Assistant  
**Descripción:**  
Consolidación total de la lógica de negocio en una rama limpia basada en el último estado de `main`.
- **Integración de Módulo Transportista**: Se añadió el módulo completo bajo `BA.Backend.Application/Transportista` incluyendo Commands, Queries, DTOs, Handlers e Interfaces.
- **Restauración de Módulo Tecnico**: Se re-incorporaron todos los archivos del perfil técnico que estaban ausentes o desactualizados.
- **Sincronización con Main**: Se trajeron los últimos cambios de la rama principal, incluyendo el módulo de Cliente y flujos de pedidos.
- **Hotfix Técnico**: Corrección en `BA.Backend.Application/Exceptions/ValidationExeption.cs` añadiendo un constructor para mensajes de texto plano, necesario para las validaciones de los Handlers.
- **Verificación**: El proyecto fue compilado exitosamente con `dotnet build`.

---

### [cf66870] - feat: implementacion completa de flujos de pedidos y asistencia técnica
**Fecha:** 2026-03-23  
**Autor:** (Base Main)  
**Descripción:**  
Commit base proveniente del `main` oficial que incluye:
- Lógica de pedidos para el cliente.
- Integración con servicios de asistencia técnica.
- Estructura base de infraestructura y WebAPI.
