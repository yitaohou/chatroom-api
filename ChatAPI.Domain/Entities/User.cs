namespace ChatAPI.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<Room> CreatedRooms { get; set; } = new List<Room>();
    public ICollection<UserRoom> UserRooms { get; set; } = new List<UserRoom>();
}
