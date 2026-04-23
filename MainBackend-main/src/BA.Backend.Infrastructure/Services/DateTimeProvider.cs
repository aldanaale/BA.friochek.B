using BA.Backend.Application.Common.Interfaces;

namespace BA.Backend.Infrastructure.Services;

/// <summary>
/// Implementacion real de IDateTimeProvider que usa DateTime.UtcNow del sistema.
/// En tests se puede inyectar una implementacion mock con fecha fija.
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateOnly UtcToday => DateOnly.FromDateTime(DateTime.UtcNow);
}
