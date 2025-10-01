using ChatAPI.Domain.Entities;

namespace ChatAPI.Application.Interfaces;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id);
    Task<Room?> GetByIdWithMembersAsync(Guid id);
    Task<IEnumerable<Room>> GetAllAsync();
    Task<IEnumerable<Room>> GetUserRoomsAsync(Guid userId);
    Task<Room> AddAsync(Room room);
    Task UpdateAsync(Room room);
    Task DeleteAsync(Guid id);
    Task<bool> IsUserMemberAsync(Guid userId, Guid roomId);
    Task AddUserToRoomAsync(Guid userId, Guid roomId);
    Task RemoveUserFromRoomAsync(Guid userId, Guid roomId);
}
