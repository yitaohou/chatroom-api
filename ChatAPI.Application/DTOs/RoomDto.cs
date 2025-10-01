namespace ChatAPI.Application.DTOs;

public class RoomDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public UserDto CreatedBy { get; set; } = null!;
    public int MemberCount { get; set; }
}

public class RoomDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public UserDto CreatedBy { get; set; } = null!;
    public List<RoomMemberDto> Members { get; set; } = new();
}

public class RoomMemberDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class CreateRoomDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
