using ChatAPI.Application.DTOs;
using ChatAPI.Application.Interfaces;
using ChatAPI.Domain.Entities;

namespace ChatAPI.Application.Services;

public class RoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;

    public RoomService(IRoomRepository roomRepository, IUserRepository userRepository)
    {
        _roomRepository = roomRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync()
    {
        var rooms = await _roomRepository.GetAllAsync();
        
        return rooms.Select(r => new RoomDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            CreatedAt = r.CreatedAt,
            CreatedBy = new UserDto
            {
                Id = r.CreatedBy.Id,
                Username = r.CreatedBy.Username,
                Email = r.CreatedBy.Email,
                CreatedAt = r.CreatedBy.CreatedAt
            },
            MemberCount = r.UserRooms.Count
        });
    }

    public async Task<RoomDetailDto?> GetRoomByIdAsync(Guid roomId)
    {
        var room = await _roomRepository.GetByIdWithMembersAsync(roomId);
        
        if (room == null)
        {
            return null;
        }

        return new RoomDetailDto
        {
            Id = room.Id,
            Name = room.Name,
            Description = room.Description,
            CreatedAt = room.CreatedAt,
            CreatedBy = new UserDto
            {
                Id = room.CreatedBy.Id,
                Username = room.CreatedBy.Username,
                Email = room.CreatedBy.Email,
                CreatedAt = room.CreatedBy.CreatedAt
            },
            Members = room.UserRooms.Select(ur => new RoomMemberDto
            {
                Id = ur.User.Id,
                Username = ur.User.Username,
                JoinedAt = ur.JoinedAt
            }).ToList()
        };
    }

    public async Task<RoomDto> CreateRoomAsync(CreateRoomDto dto, Guid createdById)
    {
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
            CreatedById = createdById
        };

        await _roomRepository.AddAsync(room);
        
        // Automatically add creator to the room
        await _roomRepository.AddUserToRoomAsync(createdById, room.Id);

        // Reload with creator info
        var createdRoom = await _roomRepository.GetByIdAsync(room.Id);

        return new RoomDto
        {
            Id = createdRoom!.Id,
            Name = createdRoom.Name,
            Description = createdRoom.Description,
            CreatedAt = createdRoom.CreatedAt,
            CreatedBy = new UserDto
            {
                Id = createdRoom.CreatedBy.Id,
                Username = createdRoom.CreatedBy.Username,
                Email = createdRoom.CreatedBy.Email,
                CreatedAt = createdRoom.CreatedBy.CreatedAt
            },
            MemberCount = 1
        };
    }

    public async Task JoinRoomAsync(Guid userId, Guid roomId)
    {
        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room == null)
        {
            throw new InvalidOperationException("Room not found");
        }

        var isMember = await _roomRepository.IsUserMemberAsync(userId, roomId);
        if (isMember)
        {
            throw new InvalidOperationException("Already a member of this room");
        }

        await _roomRepository.AddUserToRoomAsync(userId, roomId);
    }

    public async Task LeaveRoomAsync(Guid userId, Guid roomId)
    {
        var isMember = await _roomRepository.IsUserMemberAsync(userId, roomId);
        if (!isMember)
        {
            throw new InvalidOperationException("Not a member of this room");
        }

        await _roomRepository.RemoveUserFromRoomAsync(userId, roomId);
    }

    public async Task DeleteRoomAsync(Guid roomId, Guid userId)
    {
        var room = await _roomRepository.GetByIdAsync(roomId);
        
        if (room == null)
        {
            throw new InvalidOperationException("Room not found");
        }

        if (room.CreatedById != userId)
        {
            throw new UnauthorizedAccessException("Only room creator can delete the room");
        }

        await _roomRepository.DeleteAsync(roomId);
    }

    public async Task<IEnumerable<RoomDto>> GetUserRoomsAsync(Guid userId)
    {
        var rooms = await _roomRepository.GetUserRoomsAsync(userId);
        
        return rooms.Select(r => new RoomDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            CreatedAt = r.CreatedAt,
            CreatedBy = new UserDto
            {
                Id = r.CreatedBy.Id,
                Username = r.CreatedBy.Username,
                Email = r.CreatedBy.Email,
                CreatedAt = r.CreatedBy.CreatedAt
            },
            MemberCount = r.UserRooms.Count
        });
    }
}
