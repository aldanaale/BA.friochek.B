using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BA.Backend.Infrastructure.Repositories;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly ApplicationDbContext _context;
    public UserSessionRepository(ApplicationDbContext context) => _context = context;

    public async Task<UserSession?> GetActiveSessionByDeviceAsync(Guid userId, string deviceId) =>
        await _context.UserSessions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.DeviceId == deviceId && s.IsActive);

    public async Task<IEnumerable<UserSession>> GetActiveSessionsByUserAsync(Guid userId) =>
        await _context.UserSessions.AsNoTracking()
            .Where(s => s.UserId == userId && s.IsActive).ToListAsync();

    public async Task<UserSession?> GetSessionByIdAsync(Guid sessionId) =>
        await _context.UserSessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sessionId);

    // FIX: era s.JwtToken (propiedad que no existe) → s.AccessToken
    public async Task<UserSession?> GetSessionByTokenAsync(string token) =>
        await _context.UserSessions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.AccessToken == token && s.IsActive);

    public async Task<UserSession> CreateSessionAsync(UserSession session)
    {
        session.CreatedAt = DateTime.UtcNow;
        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<bool> InvalidateSessionAsync(Guid sessionId, string reason)
    {
        var s = await _context.UserSessions.FirstOrDefaultAsync(x => x.Id == sessionId);
        if (s is null) return false;
        s.IsActive = false; s.ClosedAt = DateTime.UtcNow; s.ClosureReason = reason;
        _context.UserSessions.Update(s);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> InvalidateAllUserSessionsAsync(Guid userId, string reason)
    {
        var list = await _context.UserSessions.Where(x => x.UserId == userId && x.IsActive).ToListAsync();
        foreach (var s in list) { s.IsActive = false; s.ClosedAt = DateTime.UtcNow; s.ClosureReason = reason; }
        if (list.Count > 0) { _context.UserSessions.UpdateRange(list); await _context.SaveChangesAsync(); }
        return list.Count;
    }

    public async Task<int> InvalidatePreviousSessionAsync(Guid userId, string newDeviceId)
    {
        var list = await _context.UserSessions
            .Where(x => x.UserId == userId && x.DeviceId != newDeviceId && x.IsActive).ToListAsync();
        foreach (var s in list) { s.IsActive = false; s.ClosedAt = DateTime.UtcNow; s.ClosureReason = "Nueva sesion en otro dispositivo"; }
        if (list.Count > 0) { _context.UserSessions.UpdateRange(list); await _context.SaveChangesAsync(); }
        return list.Count;
    }

    public async Task<bool> UpdateLastActivityAsync(Guid sessionId)
    {
        var s = await _context.UserSessions.FirstOrDefaultAsync(x => x.Id == sessionId);
        if (s is null) return false;
        s.LastActivityAt = DateTime.UtcNow;
        _context.UserSessions.Update(s);
        await _context.SaveChangesAsync();
        return true;
    }
}
