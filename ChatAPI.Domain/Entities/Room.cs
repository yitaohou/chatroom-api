namespace ChatAPI.Domain.Entities;

public class Room
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedById { get; set; }

    // Navigation properties
    public User CreatedBy { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<UserRoom> UserRooms { get; set; } = new List<UserRoom>();
}
