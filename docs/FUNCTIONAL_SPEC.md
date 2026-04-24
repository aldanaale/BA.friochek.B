# Especificación Funcional — BA.FrioCheck

Este documento detalla las capacidades lógicas del sistema, las reglas de negocio aplicadas y el estado actual de cada funcionalidad.

---

## 👥 1. Funcionalidades por Rol

### 🛡️ Administrador (PlatformAdmin)
- **Gestión Global**: Crear, editar y bloquear usuarios de cualquier empresa (Tenant).
- **Control de Activos**: Alta y baja de Coolers y enrolamiento de Tags NFC.
- **Auditoría**: Monitoreo de sesiones activas y huellas digitales de dispositivos.
- **Reporting**: Visualización de estadísticas agregadas (Mermas, Tickets, Pedidos).

### 🛒 Retailer (Cliente)
- **Dashboard en Tiempo Real**: Vista de coolers instalados y estado de pedidos.
- **Pedidos Rápidos**: Generación de órdenes de reposición basadas en el stock de la tienda.
- **Soporte**: Reporte de fallas técnicas y seguimiento de tickets.
- **Validación NFC**: Escaneo de tags para acceso a pedidos específicos de un cooler.

### 🚚 Transportista (Delivery)
- **Hoja de Ruta**: Listado dinámico de paradas asignadas por el sistema.
- **Entrega Certificada**: Confirmación de llegada y entrega física mediante escaneo NFC.
- **Balanceo de Stock**: Descuento automático de inventario al momento de la entrega.
- **Gestión de Incidencias**: Reporte de mermas con evidencia fotográfica.

### 🔧 Técnico (Technician)
- **Gestión de Tickets**: Recepción de alertas de falla técnica y cierre de reparaciones.
- **Mantenimiento NFC**: Reparación o re-enrolamiento de tags dañados.

---

## 📏 2. Matriz de Estado Funcional

| Funcionalidad | Estado | Detalle |
| :--- | :--- | :--- |
| **Aislamiento Multi-tenant** | ✅ Operativo | Seguridad por `TenantId` en cada query. |
| **Login por Email (Auto-tenant)** | ✅ Operativo | El sistema detecta la empresa vía correo. |
| **Descuento de Stock en Venta** | ✅ Operativo | Sincronizado con `RegisterDelivery`. |
| **Validación NFC en Terreno** | ✅ Operativo | Token temporal de 10 min por escaneo. |
| **Cloud Storage (Fotos)** | ⏳ Pendiente | Actualmente usa `LocalFileStorage`. |
| **Notificaciones Push** | ⏳ Pendiente | Planeado vía Firebase (FCM). |
| **Generación de PDFs** | ⏳ Pendiente | Generación de certificados de entrega. |

---

## 🛠️ 3. Reglas de Negocio Críticas

1.  **Seguridad de Acceso**: Un transportista no puede ver pedidos de otra ruta. Un técnico no puede ver pedidos de un transportista.
2.  **Validación de Inquilino**: Cada acción de escritura es validada contra el `TenantId` del JWT. Ningún usuario puede inyectar un ID de otra empresa.
3.  **Integridad de Stock**: Si el pedido excede el stock disponible, el sistema debe alertar (actualmente permite el descuento negativo para evitar bloqueos operativos, pendiente control estricto).
4.  **Sesión Única**: Si un usuario inicia sesión en un dispositivo nuevo, la sesión del dispositivo anterior es invalidada automáticamente.
