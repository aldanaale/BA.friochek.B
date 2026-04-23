# Módulo Tecnico 
 
El módulo "Tecnico" maneja las operaciones de técnicos en la API, incluyendo reporte de fallas, cambio de repuestos, subida de evidencias, validación NFC y certificación de reparaciones. 
 
## Funcionalidades 
- Reportar fallas en máquinas. 
- Cambiar repuestos y reportar faltas de stock. 
- Subir evidencias fotográficas. 
- Validar códigos NFC. 
- Certificar reparaciones y obtener cierres. 
 
## Endpoints 
- GET /api/tecnico/tickets: Obtener tickets asignados. 
- POST /api/tecnico/falla: Reportar falla. 
- POST /api/tecnico/cierre: Certificar reparación. 
(Requiere rol "Tecnico" en JWT). 
