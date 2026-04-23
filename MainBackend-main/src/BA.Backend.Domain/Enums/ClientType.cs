namespace BA.Backend.Domain.Enums;

/// <summary>
/// Sub-clasificación del rol Cliente según el tipo de negocio.
/// Se almacena como TINYINT nullable en la tabla Users.
/// Solo aplica cuando User.Role == UserRole.Cliente.
/// </summary>
public enum ClientType : byte
{
    /// <summary>
    /// Minorista: kiosco, botillería, almacén, minimarket.
    /// Compra directa a precio fijo, MOQ bajo, pago contado o a 7 días.
    /// </summary>
    Retail = 1,

    /// <summary>
    /// Mayorista/distribuidor que revende.
    /// Alto volumen, pedido largo (30-60 días), descuento por volumen, multi-local.
    /// </summary>
    Wholesale = 2,

    /// <summary>
    /// Cadena retail centralizada: supermercado, minisuper, multitienda.
    /// Multi-branch, contrato marco, factura central.
    /// </summary>
    Chain = 3,

    /// <summary>
    /// Hotel, restaurante, café, casino.
    /// Necesidad de producto premium, entrega con agenda programada.
    /// </summary>
    Horeca = 4,

    /// <summary>
    /// Hospital, colegio, casino institucional.
    /// Compra por licitación o contrato anual, factura diferida.
    /// </summary>
    Institutional = 5,

    /// <summary>
    /// Operador de máquinas automáticas o expendedoras.
    /// Auto-reposición, ruta fija, sin intervención manual por pedido.
    /// </summary>
    Vending = 6
}
