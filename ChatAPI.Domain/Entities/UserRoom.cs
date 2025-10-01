namespace ChatAPI.Domain.Entities;

/// <summary>
/// Join table for many-to-many relationship between Users and Rooms
/// </summary>
public class UserRoom
{
    public Guid UserId { get; set; }
    public Guid RoomId { get; set; }
    public DateTime JoinedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Room Room { get; set; } = null!;
}
