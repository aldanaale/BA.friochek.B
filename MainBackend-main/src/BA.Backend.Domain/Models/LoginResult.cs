using BA.Backend.Domain.Entities;

namespace BA.Backend.Domain.Models;

public class LoginResult
{
    public User? User { get; set; }
    public Tenant? Tenant { get; set; }
    public List<UserSession> ActiveSessions { get; set; } = new();
    public Store? Store { get; set; }
    public List<Cooler> Coolers { get; set; } = new();
}
