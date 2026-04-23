# Módulo Transportista

## Propósito
El módulo de **Transportista** gestiona las operaciones diarias de los operarios que recorren rutas de locales para realizar entregas de productos, retiro de mermas y reporte de incidencias técnicas. El sistema asegura la trazabilidad mediante validaciones NFC obligatorias en cada punto de contacto con las máquinas.

## Commands
- **RegisterDeliveryCommand**: Registra la entrega efectiva de productos en una parada de la ruta. Requiere confirmación mediante escaneo NFC.
- **RegisterWastePickupCommand**: Registra el retiro de productos dañados o vencidos (merma). Requiere evidencia fotográfica y validación NFC.
- **CreateSupportTicketCommand**: Genera un ticket de asistencia técnica desde el sitio del local, certificado por la ubicación del tag NFC.
- **ValidateNfcTagCommand**: Operación técnica para verificar la correspondencia entre un tag escaneado y una máquina específica.

## Queries
- **GetDailyRouteQuery**: Obtiene la lista de paradas y máquinas asignadas para el día actual.
- **GetMachineByNfcTagQuery**: Recupera la información detallada de una máquina tras el escaneo inicial de su tag.
- **GetPendingDeliveriesByRouteQuery**: Filtra la ruta actual para mostrar únicamente los puntos de entrega aún pendientes.
- **GetMachineMovementHistoryQuery**: Consulta el historial de interacciones (entregas, mermas, tickets) de una máquina específica.
- **GetPendingTicketsByRouteQuery**: Lista las alertas y tickets activos en las máquinas de la ruta asignada.

## Dependencias
El módulo se apoya en las siguientes abstracciones para mantener la independencia de la infraestructura:
- **ITransportistaRepository**: Acceso a datos de rutas, máquinas y persistencia de operaciones.
- **INfcValidationService**: Lógica de validación de tags y certificación de presencia física.
- **IPhotoStorageService**: Gestión de almacenamiento de evidencias fotográficas para mermas y tickets.

## Reglas de Negocio Principales
1. **Validación NFC Obligatoria**: No se pueden registrar entregas, mermas ni tickets sin un escaneo válido del tag NFC de la máquina.
2. **Evidencia de Merma**: El retiro de productos dañados requiere obligatoriamente la subida de una foto como evidencia del estado de los productos.
3. **Calidad de Información**: Los tickets de soporte deben incluir una descripción detallada de al menos 20 caracteres para facilitar el diagnóstico previo del técnico.
4. **Cantidades no Negativas**: Todas las validaciones de entrada aseguran que las cantidades de productos entregados o retirados sean iguales o mayores a cero.
