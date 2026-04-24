namespace BA.Backend.Application.Common.Interfaces;

public interface IGeoLocationService
{
    /// <summary>
    /// Calcula la distancia en metros entre dos puntos geográficos usando la fórmula de Haversine.
    /// </summary>
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2);

    /// <summary>
    /// Verifica si un punto está dentro de un radio determinado (en metros) respecto a otro punto.
    /// </summary>
    bool IsWithinRange(double lat1, double lon1, double lat2, double lon2, double rangeInMeters);
}
