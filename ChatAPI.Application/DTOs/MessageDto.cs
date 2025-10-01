namespace ChatAPI.Application.DTOs;

public class MessageDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public UserDto User { get; set; } = null!;
    public Guid RoomId { get; set; }
}

public class SendMessageDto
{
    public Guid RoomId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class MessageHistoryDto
{
    public List<MessageDto> Messages { get; set; } = new();
    public bool HasMore { get; set; }
}
