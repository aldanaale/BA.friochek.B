namespace BA.Backend.Domain.Entities;

public class ActiveSession
{
    public string SessionId { get; set; } = null!;
    public Guid UserId { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}