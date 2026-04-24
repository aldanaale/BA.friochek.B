# PROJECT MAESTRO — BA.FrioCheck

Este documento es la fuente única de verdad para la gestión, técnica e historia del proyecto FrioCheck.

---

## 📖 1. MEMORIA Y EVOLUCIÓN (HISTORIA)

### Hitos de Estabilización (2026)
*   **Febrero - Marzo**: Re-ingeniería de Base de Datos y Fix de Multi-tenancy. El Health Score subió de 60% a 98%.
*   **Abril (Hardening)**: Implementación de seguridad industrial (CORS, Swagger, JWT Environment Variables).
*   **Abril (Logística)**: Implementación real del módulo de transporte con descuento automático de Stock.

### Hallazgos de Seguridad Resueltos
*   **CORS**: Transición de política abierta a Whitelist restrictiva.
*   **Exposición**: Swagger UI bloqueado en producción.
*   **Secretos**: Eliminación de claves hardcodeadas; uso de Variables de Entorno.

---

## 🏗️ 2. ARQUITECTURA TÉCNICA

### Clean Architecture
El sistema sigue un flujo de dependencias hacia el `Domain`:
`WebAPI` → `Application` → `Domain` ← `Infrastructure`.

### Patrones Clave
*   **CQRS**: MediatR separa comandos de lectura/escritura.
*   **DDD**: Entidades ricas con validación interna y Factory Methods.
*   **Multi-tenancy**: Aislamiento por `TenantId` con filtros globales automáticos.
*   **Performance**: Uso de Dapper para dashboards ultrarrápidos (<300ms).

---

## 🚀 3. OPERACIONES Y DESARROLLO

### Inicio Rápido
1. `dotnet restore`
2. Configurar SQL Express (`BD_FC`).
3. `dotnet run --project src/BA.Backend.WebAPI` (Puerto 5003).

### Suite de Pruebas
Ejecutar `./test.ps1` para validación completa. El objetivo es mantener siempre un score de **100/100**.

### Credenciales (Seed)
*   **Admin**: `user1_1@savory-chile.cl` (Pass: `DevPass123!`)
*   **Cliente**: `user2_2@savory-chile.cl`
*   **Transportista**: `user3_3@savory-chile.cl`
*   **Técnico**: `user4_2@savory-chile.cl`

---

## 📊 4. ESTADO DE AVANCE Y ROADMAP

### Checklist Funcional de Capacidades
- [x] Multi-tenancy sólido y aislamiento de datos.
- [x] Seguridad Industrial (JWT, CORS, Rate Limit).
- [x] Logística Real (Entregas con descuento de Stock y NFC).
- [x] Dashboards por rol funcionales.
- [ ] **Fase 3**: Almacenamiento en Azure/AWS (Mermas y Certificados).
- [ ] **Fase 3**: Notificaciones Push (Firebase).
- [ ] **Fase 3**: Generación de PDFs (Certificados de Entrega).

### Deuda Técnica Priorizada
1. Corregir typo `TransportistId` por `TransportistaId`.
2. Aumentar cobertura unitaria al 60%.
3. Implementar Correlation IDs en Serilog.
