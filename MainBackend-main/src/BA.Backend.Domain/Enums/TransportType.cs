namespace BA.Backend.Domain.Enums;

/// <summary>
/// Sub-clasificación del rol Transportista según la función de transporte.
/// Se almacena como TINYINT nullable en Users y en la tabla Transportistas.
/// Solo aplica cuando User.Role == UserRole.Transportista.
/// </summary>
public enum TransportType : byte
{
    /// <summary>
    /// Transportista en vehículo refrigerado.
    /// Registra temperatura de viaje (cadena de frío), log de rutas.
    /// </summary>
    ProductCarrier = 1,

    /// <summary>
    /// Traslada, instala y retira coolers.
    /// Emite certificado de instalación, vincula NFC asset.
    /// </summary>
    MachineCarrier = 2,

    /// <summary>
    /// Flete internacional puerto → bodega. [FUTURO]
    /// Maneja incoterms (CIF/FOB), documentos aduaneros, BL.
    /// Se modela pero los endpoints se implementan en Fase posterior.
    /// </summary>
    FreightForwarder = 3,

    /// <summary>
    /// Repartidor urbano en moto o furgón pequeño.
    /// Solo ve su ruta del día, confirmación de entrega simple.
    /// </summary>
    LastMile = 4
}
