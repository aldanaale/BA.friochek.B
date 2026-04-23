using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BA.Backend.WebAPI.Swagger;

/// <summary>
/// Define el orden y la descripción de cada sección (tag) en Swagger UI.
/// Los tags siguen el nombre exacto del controller, en el mismo orden
/// que genera Swashbuckle por defecto en MainBackend-main (referencia).
/// Sin este filtro Swashbuckle ordena alfabéticamente — aquí lo fijamos
/// poniendo Auth primero (punto de entrada obligatorio) y el resto en orden lógico.
/// </summary>
public class TagOrderDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags = new List<OpenApiTag>
        {
            new() { Name = "Auth",                  Description = "Gestión de Sesiones y Seguridad: Login, logout y recuperación de contraseña. Punto de entrada principal para obtener el token JWT." },
            new() { Name = "Admin",                 Description = "Panel de Administración: Dashboard de estadísticas, gestión de mermas y supervisión de solicitudes de soporte. Requiere elevación de privilegios (Admin)." },
            new() { Name = "Users",                 Description = "Gestión de Identidades: Control de usuarios dentro del tenant, auditoría de acceso, bloqueo y desbloqueo de cuentas." },
            new() { Name = "Cliente",               Description = "Portal de Autoservicio Retailer: Catálogo dinámico de productos, gestión del ciclo de órdenes y tickets de soporte técnico." },
            new() { Name = "Coolers",               Description = "Inventario de Activos (Coolers): Gestión del ciclo de vida de los equipos de frío, monitoreo de estado y asignación geográfica." },
            new() { Name = "Nfc",                   Description = "Ecosistema NFC: Validación, enrolamiento y vinculación de Tags NFC para la trazabilidad física de los activos en campo." },
            new() { Name = "Transporte",            Description = "Logística de Última Milla: Gestión de rutas inteligentes, seguimiento de entregas en tiempo real y validación de campo mediante NFC." },
            new() { Name = "Stores",                Description = "Puntos de Venta (Stores): Administración de ubicaciones físicas, sucursales y vinculación con equipos de frío." },
            new() { Name = "Tecnico",               Description = "Operaciones de Campo (SDA): Ejecución de mantenimientos preventivos/correctivos, reparación de fallas y reporte de actividades técnicas." },
            new() { Name = "Comercial",             Description = "Gestión Comercial y Ventas: Seguimiento de clientes, notas comerciales y dashboard de rendimiento de ventas." },
            new() { Name = "Supervisor",            Description = "Supervisión Operativa: Monitoreo de cumplimiento de rutas, auditoría de entregas y gestión de excepciones." },
            new() { Name = "Platform",              Description = "Administración Global: Configuración del sistema, gestión de multi-inquilinos (Tenants) y parámetros maestros." },
            new() { Name = "Ping",                  Description = "Disponibilidad del Sistema: Monitor de salud y latencia de la API. Punto de acceso público para monitoreo externo." },
        };
    }
}
