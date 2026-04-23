using BA.Backend.Application.Transportista.Interfaces;
using BA.Backend.Application.Transportista.DTOs;
using BA.Backend.Application.Transportista.Commands;
using BA.Backend.Application.Transportista;
using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Infrastructure;
using BA.Backend.Application.Tecnico.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Dapper;
using BA.Backend.Infrastructure.Settings;
using System.Security.Claims;

namespace BA.Backend.Infrastructure.Repositories;

/// <summary>
/// Repositorio unificado de Transportista que maneja tanto operaciones de Dominio (EF Core)
/// como consultas de Aplicación optimizadas (Dapper).
/// </summary>
public class TransportistaRepository : 
    BA.Backend.Application.Transportista.Interfaces.ITransportistaRepository,
    BA.Backend.Domain.Repositories.ITransportistaRepository
{
    private readonly string _connectionString;
    private readonly ApplicationDbContext _context;

    public TransportistaRepository(DatabaseSettings settings, ApplicationDbContext context)
    {
        _connectionString = settings.DefaultConnection;
        _context = context;
    }

    #region Implementación de Dominio (EF Core)

    public async Task<IEnumerable<Transportista>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct)
    {
        return await _context.Transportistas
            .Include(t => t.User)
            .Where(t => t.TenantId == tenantId)
            .ToListAsync(ct);
    }

    public async Task<Transportista?> GetByIdAsync(Guid userId, CancellationToken ct)
    {
        return await _context.Transportistas
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.UserId == userId, ct);
    }

    public async Task AddAsync(Transportista transportista, CancellationToken ct)
    {
        _context.Transportistas.Add(transportista);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Transportista transportista, CancellationToken ct)
    {
        _context.Transportistas.Update(transportista);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken ct)
    {
        var transportista = await _context.Transportistas.FirstOrDefaultAsync(t => t.UserId == userId, ct);
        if (transportista != null)
        {
            _context.Transportistas.Remove(transportista);
            await _context.SaveChangesAsync(ct);
        }
    }

    #endregion

    #region Implementación de Aplicación (Dapper / Consultas)

    public async Task<List<TransportistaRouteStopDto>> GetDailyRouteAsync(Guid transportistaId, DateTime routeDate, Guid tenantId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string sql = @"
            SELECT 
                rs.StoreId AS StoreId,
                s.Name AS StoreName,
                s.Address AS Address,
                CAST(CASE WHEN EXISTS (SELECT 1 FROM RouteStops rs2 WHERE rs2.RouteId = r.Id AND rs2.Status = 'Pendiente') THEN 1 ELSE 0 END AS BIT) AS HasPendingDelivery,
                CAST(0 AS BIT) AS HasActiveAlert
            FROM Routes r
            INNER JOIN RouteStops rs ON r.Id = rs.RouteId
            LEFT JOIN Stores s ON rs.StoreId = s.Id
            WHERE r.TransportistaId = @TransportistaId 
              AND r.TenantId = @TenantId
              AND CAST(r.Date AS DATE) = CAST(@RouteDate AS DATE)
            ORDER BY rs.StopOrder ASC";

        var rows = await connection.QueryAsync<TransportistaRouteStopDto>(sql, new { TransportistaId = transportistaId, RouteDate = routeDate, TenantId = tenantId });
        return (rows ?? Enumerable.Empty<TransportistaRouteStopDto>()).ToList();
    }

    public async Task<List<TransportistaRouteDto>> GetPendingRouteStopsAsync(Guid transportistaId, Guid tenantId)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            SELECT 
                rs.Id AS RouteStopId,
                s.Name AS StoreName,
                s.Address AS StoreAddress,
                '' AS StoreCity,
                rs.Status AS Status
            FROM RouteStops rs
            INNER JOIN Routes r ON rs.RouteId = r.Id
            LEFT JOIN Stores s ON rs.StoreId = s.Id
            WHERE r.TransportistaId = @TransportistaId 
              AND r.TenantId = @TenantId
              AND rs.Status = 'Pendiente'
            ORDER BY rs.StopOrder ASC";

        var result = await connection.QueryAsync<TransportistaRouteDto>(sql, new { TransportistaId = transportistaId, TenantId = tenantId });
        return (result ?? Enumerable.Empty<TransportistaRouteDto>()).ToList();
    }

    public async Task<CoolerDetailDto> GetCoolerByNfcTagAsync(string nfcTagId)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            SELECT 
                c.Id AS CoolerId,
                c.SerialNumber,
                c.Model,
                c.Status,
                s.Name AS StoreName
            FROM Coolers c
            INNER JOIN Stores s ON c.StoreId = s.Id
            INNER JOIN NfcTags n ON n.CoolerId = c.Id
            WHERE n.TagId = @NfcTagId AND n.IsDeleted = 0";

        var result = await connection.QueryFirstOrDefaultAsync<CoolerDetailDto>(sql, new { NfcTagId = nfcTagId });
        return result ?? throw new KeyNotFoundException($"No cooler found for NFC tag: {nfcTagId}");
    }

    public async Task ValidateNfcTagAsync(string nfcTagId, Guid expectedCoolerId)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            SELECT COUNT(1) FROM NfcTags 
            WHERE TagId = @TagId AND CoolerId = @CoolerId AND IsActive = 1";

        var exists = await connection.ExecuteScalarAsync<int>(sql, new { TagId = nfcTagId, CoolerId = expectedCoolerId });
        if (exists == 0)
        {
            throw new InvalidOperationException("NFC tag does not match expected cooler");
        }
    }

    public async Task<DeliveryResultDto> RegisterDeliveryAsync(RegisterDeliveryCommand command)
    {
        // 1. Obtener la parada de la ruta
        var routeStop = await _context.RouteStops
            .FirstOrDefaultAsync(rs => rs.Id == command.RouteStopId);

        if (routeStop == null)
            throw new KeyNotFoundException($"RouteStop {command.RouteStopId} not found");

        // 2. Marcar como completado
        routeStop.MarkAsCompleted();
        
        // 3. Descontar Stock Real de la BD (Fase 2)
        foreach (var delivery in command.Deliveries)
        {
            foreach (var productDelivery in delivery.Products)
            {
                var product = await _context.Products.FindAsync(productDelivery.ProductId);
                if (product != null)
                {
                    product.Stock -= productDelivery.QuantityDelivered;
                    if (product.Stock < 0) product.Stock = 0; // Prevenir stock negativo
                }
            }
        }

        await _context.SaveChangesAsync();

        return new DeliveryResultDto 
        { 
            DeliveryId = routeStop.Id,
            CoolersIncluded = command.Deliveries.Count,
            TotalProductsDelivered = command.Deliveries.SelectMany(d => d.Products).Sum(p => p.QuantityDelivered),
            DeliveredAt = DateTime.UtcNow,
            Status = "Entregado"
        };
    }

    public async Task<WastePickupResultDto> RegisterWastePickupAsync(RegisterWastePickupCommand command)
    {
        foreach (var item in command.Items)
        {
            var merma = Merma.Create(
                command.TransportistaId, // El repo usará el ID del transportista como tenant o el tenant del transportista
                command.TransportistaId,
                command.CoolerId,
                item.ProductId,
                "Producto en Merma", // El nombre se puede resolver si se desea más detalle
                item.QuantityRemoved,
                "Merma reportada en ruta",
                command.PhotoEvidenceUrl,
                "Reporte desde app móvil",
                command.ConfirmationNfcTagId
            );
            
            // Necesitamos el TenantId real del transportista
            var transportista = await _context.Transportistas.FindAsync(command.TransportistaId);
            if (transportista != null) merma.TenantId = transportista.TenantId;

            _context.Mermas.Add(merma);
        }

        await _context.SaveChangesAsync();
        return new WastePickupResultDto(
            Guid.NewGuid(), 
            command.CoolerId, 
            command.Items.Sum(i => i.QuantityRemoved), 
            command.PhotoEvidenceUrl, 
            DateTime.UtcNow
        );
    }

    public async Task<SupportTicketResultDto> CreateSupportTicketAsync(CreateSupportTicketCommand command)
    {
        var transportista = await _context.Transportistas.FindAsync(command.TransportistaId);
        if (transportista == null) throw new UnauthorizedAccessException("Transportista no válido");

        var ticket = new TechSupportRequest
        {
            Id = Guid.NewGuid(),
            TenantId = transportista.TenantId,
            UserId = command.TransportistaId,
            CoolerId = command.CoolerId,
            NfcTagId = command.NfcTagId,
            FaultType = command.Category.ToString(),
            Description = command.Description,
            PhotoUrls = command.PhotoEvidenceUrl ?? "[]",
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            Status = "Pendiente",
            CreatedAt = DateTime.UtcNow
        };

        _context.TechSupportRequests.Add(ticket);
        await _context.SaveChangesAsync();

        return new SupportTicketResultDto(
            ticket.Id,
            $"TKT-{ticket.Id.ToString()[..8].ToUpper()}",
            command.CoolerId,
            command.Category,
            TicketStatus.Open,
            ticket.CreatedAt
        );
    }

    public async Task<List<MovementSummaryDto>> GetCoolerHistoryAsync(Guid coolerId, DateTime? from, DateTime? to, MovementType? type, int page, int size)
    {
        return new List<MovementSummaryDto>();
    }

    public async Task<List<SupportTicketResultDto>> GetPendingTicketsByRouteAsync(Guid transportistaId, DateTime routeDate)
    {
        return new List<SupportTicketResultDto>();
    }

    #endregion
}

public class NfcValidationService : INfcValidationService
{
    private readonly string _connectionString;

    public NfcValidationService(DatabaseSettings settings)
    {
        _connectionString = settings.DefaultConnection;
    }

    public async Task ValidateTagAsync(string scannedTagId, Guid coolerId)
    {
        using var connection = new SqlConnection(_connectionString);

        const string sql = @"
            SELECT COUNT(1) FROM NfcTags 
            WHERE TagId = @TagId AND CoolerId = @CoolerId AND IsDeleted = 0";


        var exists = await connection.ExecuteScalarAsync<int>(sql, new { TagId = scannedTagId, CoolerId = coolerId });

        if (exists == 0)
        {
            throw new InvalidOperationException($"NFC tag {scannedTagId} is not registered for cooler {coolerId}");
        }
    }

    public async Task<bool> IsTagRegisteredAsync(string nfcTagId)
    {
        using var connection = new SqlConnection(_connectionString);

        const string sql = @"SELECT COUNT(1) FROM NfcTags WHERE TagId = @TagId AND IsDeleted = 0";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { TagId = nfcTagId });

        return count > 0;
    }
}
