namespace ChatAPI.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public Guid UserId { get; set; }
    public Guid RoomId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Room Room { get; set; } = null!;
}
