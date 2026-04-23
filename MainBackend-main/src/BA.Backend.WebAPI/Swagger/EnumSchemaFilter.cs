using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Xml.Linq;
using System.Reflection;

namespace BA.Backend.WebAPI.Swagger;

/// <summary>
/// Enriquece los schemas de enums en Swagger agregando descripción legible
/// de cada valor: "1 = Retail — Kiosco, botillería, almacén" en vez de solo "1".
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    private static readonly Dictionary<string, string> _enumDocs = new()
    {
        // UserRole
        ["UserRole.Admin"]               = "Administrador de tenant. Gestiona usuarios, productos y reportes de su marca.",
        ["UserRole.TenantAdmin"]         = "Alias de Admin (=1). Administra una sola marca.",
        ["UserRole.Cliente"]             = "Cliente comercial. Sub-tipo definido por ClientType.",
        ["UserRole.Transportista"]       = "Actor logístico. Sub-tipo definido por TransportType.",
        ["UserRole.Tecnico"]             = "Técnico SDA en campo. Escanea NFC, registra visitas.",
        ["UserRole.PlatformAdmin"]       = "Administrador de plataforma. Acceso cross-tenant, sin filtro de TenantId.",
        ["UserRole.Supervisor"]          = "Supervisa operaciones SDA y técnicos en campo. Recibe alertas.",
        ["UserRole.EjecutivoComercial"]  = "Ejecutivo Comercial. Gestiona clientes, órdenes y pipeline de ventas.",

        // ClientType
        ["ClientType.Retail"]            = "Minorista: kiosco, botillería, almacén. Compra contado/7d, MOQ bajo.",
        ["ClientType.Wholesale"]         = "Mayorista/distribuidor. Alto volumen, crédito 30-90 días, descuento vol.",
        ["ClientType.Chain"]             = "Cadena retail centralizada. Multi-branch, contrato, factura central.",
        ["ClientType.Horeca"]            = "Hotel, restaurante, café. Premium SKU, entrega con agenda programada.",
        ["ClientType.Institutional"]     = "Hospital, colegio, casino. Licitación o contrato anual, factura diferida.",
        ["ClientType.Vending"]           = "Operador de máquinas automáticas/carros de helado. Auto-reposición, ruta fija.",

        // TransportType
        ["TransportType.ProductCarrier"] = "Camión refrigerado. Registra temperatura de viaje (cadena de frío).",
        ["TransportType.MachineCarrier"] = "Traslada, instala y retira coolers. Emite cert. instalación vía NFC.",
        ["TransportType.FreightForwarder"] = "[FUTURO] Flete internacional puerto→bodega. CIF/FOB, aduana, BL.",
        ["TransportType.LastMile"]       = "Repartidor urbano moto/furgón. Solo ve su ruta del día.",
    };

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum) return;

        var enumName = context.Type.Name;
        var enumValues = Enum.GetValues(context.Type);

        var description = new System.Text.StringBuilder();
        description.AppendLine($"**{enumName}** — valores disponibles:");
        description.AppendLine();

        foreach (var value in enumValues)
        {
            var name = Enum.GetName(context.Type, value)!;
            var intVal = Convert.ToInt32(value);
            var key = $"{enumName}.{name}";
            var doc = _enumDocs.TryGetValue(key, out var d) ? d : string.Empty;

            description.AppendLine($"- `{intVal}` = **{name}**{(doc.Length > 0 ? $" — {doc}" : "")}");
        }

        schema.Description = (schema.Description ?? "") + "\n\n" + description.ToString();
    }
}
