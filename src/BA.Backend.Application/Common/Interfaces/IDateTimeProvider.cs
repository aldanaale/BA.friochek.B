namespace BA.Backend.Application.Common.Interfaces;

/// <summary>
/// Abstrae DateTime.UtcNow para permitir mockeo en tests unitarios.
/// Evita los 68 usos dispersos de DateTime.UtcNow en handlers.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>Fecha y hora actual en UTC.</summary>
    DateTime UtcNow { get; }

    /// <summary>Fecha actual en UTC (sin componente de hora).</summary>
    DateOnly UtcToday { get; }
}
