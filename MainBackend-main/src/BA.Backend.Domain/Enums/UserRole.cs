namespace BA.Backend.Domain.Enums;

public enum UserRole : byte
{
    // ── Roles existentes (valores intactos para backward compat con JWT) ──────
    /// <summary>Administrador de un tenant (marca). Gestiona usuarios, productos, reportes.</summary>
    Admin = 1,

    /// <summary>Cliente comercial. Sub-clasificado por ClientType (Retail, Wholesale, Chain, Horeca, Institutional, Vending).</summary>
    Cliente = 2,

    /// <summary>Transportista. Sub-clasificado por TransportType (ProductCarrier, MachineCarrier, FreightForwarder, LastMile).</summary>
    Transportista = 3,

    /// <summary>Técnico SDA en campo. Escanea NFC, registra visitas y reparaciones.</summary>
    Tecnico = 4,

    // ── Roles nuevos ─────────────────────────────────────────────────────────
    /// <summary>Administrador de plataforma. Acceso cross-tenant, configuración global. Ignora filtro de TenantId.</summary>
    PlatformAdmin = 5,

    /// <summary>Supervisor de operaciones. Supervisa SDA y técnicos en campo, recibe alertas.</summary>
    Supervisor = 6,

    /// <summary>Ejecutivo Comercial. Gestiona relación con clientes, genera órdenes y hace seguimiento de ventas (pipeline).</summary>
    EjecutivoComercial = 7
}
