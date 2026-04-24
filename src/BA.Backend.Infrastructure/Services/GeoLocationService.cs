using BA.Backend.Application.Common.Interfaces;

namespace BA.Backend.Infrastructure.Services;

public class GeoLocationService : IGeoLocationService
{
    private const double EarthRadiusKm = 6371.0;

    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return EarthRadiusKm * c * 1000.0; // Distancia en metros
    }

    public bool IsWithinRange(double lat1, double lon1, double lat2, double lon2, double rangeInMeters)
    {
        var distance = CalculateDistance(lat1, lon1, lat2, lon2);
        return distance <= rangeInMeters;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}
